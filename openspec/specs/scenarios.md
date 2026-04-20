# Scenarios Spec

> SovereignID · openspec/specs/scenarios.md

## Core User Scenarios

---

### SC-01 · Decentralized Login (SIWE)

**Actor**: End user with MetaMask wallet
**Goal**: Authenticate to a web app without password or Google account

```
GIVEN  user has MetaMask installed with at least one account
WHEN   user clicks "Sign in with Ethereum"
THEN   app requests a nonce from Auth Service
AND    user sees a human-readable SIWE message in MetaMask
AND    user signs the message (off-chain, zero gas)
AND    backend verifies the signature via EcRecover
AND    backend issues a JWT valid for 24 hours
AND    user is logged in — no account creation required
```

**Failure paths**:
- User rejects the signature → 401, prompt to retry
- Nonce expired (>10 min) → 401, request new nonce
- Wrong domain in SIWE message → MetaMask shows phishing warning, reject

---

### SC-02 · Issue a Verifiable Credential

**Actor**: Issuer (e.g. KYC provider, university) authenticated as admin
**Goal**: Issue a signed credential to a user's DID

```
GIVEN  issuer is authenticated (API key or admin JWT)
AND    issuer knows the subject's DID (did:ethr:sepolia:0x...)
WHEN   issuer calls POST /credentials/issue with claims payload
THEN   system builds a W3C VC document
AND    system signs it with the issuer's private key (EIP-712)
AND    system returns the signed VC as JSON-LD
AND    issuer delivers the VC to the user (out of band: email, QR, direct API)
```

**Credential types supported in v1**:
- `KYCCredential` — identity verification level + country
- `EducationCredential` — degree, institution, graduation date
- `AgeCredential` — over18: boolean (no DOB exposed)

---

### SC-03 · Store Credential in Wallet

**Actor**: End user (Holder)
**Goal**: Store a received VC for later use

```
GIVEN  user received a VC (as JSON-LD string)
WHEN   user imports it into the wallet UI
THEN   wallet validates the VC structure and signature
AND    wallet stores the VC locally (localStorage / file)
AND    wallet displays issuer, type, expiry, and claims summary
AND    user can view, copy, or delete the credential
```

---

### SC-04 · Present Credential to Verifier

**Actor**: End user (Holder) wanting to access a service
**Goal**: Prove a claim without revealing full identity

```
GIVEN  user has a valid VC in their wallet
AND    verifier service requests proof of a specific claim
WHEN   user creates a Verifiable Presentation (VP)
AND    user signs the VP with their own key (proving ownership)
AND    user submits the VP to the verifier
THEN   verifier checks: issuer signature valid, VC not revoked, not expired
AND    verifier extracts only the requested claims
AND    user gains access — no personal data leaked beyond the claim
```

**Example**: User proves "KYC level = standard" to a fintech
without revealing name, country, or date of verification.

---

### SC-05 · Verify a Credential (Verifier side)

**Actor**: Verifier service (enterprise API consumer)
**Goal**: Validate a VP received from a user

```
GIVEN  verifier received a VP from a user
WHEN   verifier calls POST /credentials/verify with the VP
THEN   system resolves the issuer's DID to get public key
AND    system verifies the issuer's signature on the VC
AND    system checks the VC is not in the revocation registry
AND    system checks the VC has not expired
AND    system verifies the holder's signature on the VP
AND    returns: { valid: true, claims: { kycLevel: "standard" } }
OR     returns: { valid: false, error: "REVOKED | EXPIRED | INVALID_SIGNATURE" }
```

**Key property**: verifier never contacts the issuer.
All verification is done against public blockchain data.

---

### SC-06 · Revoke a Credential

**Actor**: Issuer
**Goal**: Invalidate a previously issued credential

```
GIVEN  issuer has previously issued a VC with id "urn:uuid:abc..."
WHEN   issuer calls POST /credentials/revoke with the credential ID
THEN   system writes the credential ID to the on-chain revocation registry
AND    all subsequent verifications of that VC return { valid: false, error: "REVOKED" }
AND    the user's copy of the VC is not modified (they still have it)
AND    the user is NOT notified automatically (revocation is silent)
```

---

## Acceptance Criteria Summary

| Scenario | Must pass for MVP |
|----------|------------------|
| SC-01 SIWE Login | Yes |
| SC-02 Issue VC | Yes |
| SC-03 Store in Wallet | Yes (minimal UI) |
| SC-04 Present VP | Yes |
| SC-05 Verify VP | Yes |
| SC-06 Revoke VC | Yes |
| Selective disclosure (ZK) | No — post-MVP |
| Multi-chain | No — post-MVP |
