# Architecture Spec

> SovereignID · openspec/specs/architecture.md

## System Overview

SovereignID is composed of three independent but composable services
that implement the W3C Trust Triangle (Issuer → Holder → Verifier).

```
┌─────────────────────────────────────────────────────────────┐
│                        CLIENT SIDE                          │
│                                                             │
│   ┌──────────────┐        ┌───────────────────────────┐    │
│   │  MetaMask /  │        │     User Credential        │   │
│   │  Wallet      │        │     Wallet (local)         │   │
│   │  (browser)   │        │     Stores VCs as JSON     │   │
│   └──────┬───────┘        └─────────────┬─────────────┘   │
│          │ signs SIWE msg               │ presents VP       │
└──────────┼───────────────────────────── ┼ ─────────────────┘
           │                              │
┌──────────▼──────────────────────────── ▼ ─────────────────┐
│                      SERVER SIDE (.NET)                     │
│                                                             │
│  ┌─────────────────┐  ┌──────────────┐  ┌───────────────┐ │
│  │  Auth Service   │  │ Issuer API   │  │ Verifier API  │ │
│  │                 │  │              │  │               │ │
│  │ POST /auth/     │  │ POST         │  │ POST          │ │
│  │   nonce         │  │ /credentials │  │ /credentials  │ │
│  │ POST /auth/     │  │   /issue     │  │   /verify     │ │
│  │   verify        │  │              │  │               │ │
│  │ → JWT session   │  │ → signed VC  │  │ → valid/error │ │
│  └────────┬────────┘  └──────┬───────┘  └───────┬───────┘ │
│           │                  │                   │         │
└───────────┼──────────────────┼───────────────────┼─────────┘
            │                  │                   │
┌───────────▼──────────────────▼───────────────────▼─────────┐
│                    BLOCKCHAIN (Sepolia)                      │
│                                                             │
│   ┌────────────────────┐    ┌────────────────────────────┐ │
│   │  DID Registry      │    │  Revocation Registry       │ │
│   │  (did:ethr)        │    │  (minimal smart contract)  │ │
│   │  Issuer public key │    │  List of revoked VC IDs    │ │
│   └────────────────────┘    └────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## Service Contracts

### Auth Service

Handles SIWE challenge-response authentication.

```
GET  /auth/nonce          → { nonce: string, expiresAt: ISO8601 }
POST /auth/verify         → { jwt: string, address: string }
     body: { siweMessage: string, signature: string }
GET  /auth/me             → { address, did } (requires JWT)
```

### Issuer API

Issues and manages Verifiable Credentials.

```
POST /credentials/issue   → VerifiableCredential (JSON-LD)
     body: { subjectDid, credentialType, claims, expiresAt? }
POST /credentials/revoke  → { revoked: true }
     body: { credentialId }
GET  /credentials/:id     → VerifiableCredential or 404
```

### Verifier API

Verifies Verifiable Presentations from holders.

```
POST /credentials/verify  → { valid: bool, claims?, error? }
     body: VerifiablePresentation (JSON-LD)
```

## Data Models

### VerifiableCredential (W3C compliant)

```json
{
  "@context": [
    "https://www.w3.org/2018/credentials/v1"
  ],
  "type": ["VerifiableCredential", "KYCCredential"],
  "id": "urn:uuid:550e8400-e29b-41d4-a716-446655440000",
  "issuer": "did:ethr:sepolia:0xISSUER_ADDRESS",
  "issuanceDate": "2026-04-19T00:00:00Z",
  "expirationDate": "2027-04-19T00:00:00Z",
  "credentialSubject": {
    "id": "did:ethr:sepolia:0xSUBJECT_ADDRESS",
    "kycLevel": "standard",
    "country": "ES",
    "verifiedAt": "2026-04-19T00:00:00Z"
  },
  "proof": {
    "type": "EthereumEip712Signature2021",
    "created": "2026-04-19T00:00:00Z",
    "verificationMethod": "did:ethr:sepolia:0xISSUER_ADDRESS#keys-1",
    "proofValue": "0x..."
  }
}
```

### VerifiablePresentation

```json
{
  "@context": ["https://www.w3.org/2018/credentials/v1"],
  "type": ["VerifiablePresentation"],
  "holder": "did:ethr:sepolia:0xHOLDER_ADDRESS",
  "verifiableCredential": [ /* one or more VCs */ ],
  "proof": {
    "type": "EthereumEip712Signature2021",
    "challenge": "random-nonce-from-verifier",
    "proofValue": "0x..."
  }
}
```

## Key Design Decisions

### Why did:ethr?
Simplest DID method on Ethereum. No custom smart contract needed for basic use.
Public key is derived directly from the Ethereum address. Resolvable without a
dedicated resolver service for basic operations.

### Why JWT for sessions?
After SIWE verification, issuing a standard JWT keeps the rest of the app
conventional. No need for every request to re-verify a blockchain signature.
Stateless, cacheable, familiar to any .NET developer.

### Why separate Issuer and Verifier services?
In production these would be deployed independently by different organizations.
Keeping them separate from day one reinforces the correct mental model and makes
the demo architecturally honest — the verifier does NOT need to call the issuer.

### Why Sepolia?
Free testnet ETH, EVM-compatible, stable, widely supported by tooling (Infura,
Alchemy, MetaMask). No cost, no risk, identical developer experience to mainnet.

## Solution Boundaries

| In scope | Out of scope |
|----------|-------------|
| EIP-4361 SIWE | ERC-4337 account abstraction |
| W3C VC Data Model 1.1 | W3C VC Data Model 2.0 (BBS+) |
| did:ethr | did:web, did:ion, did:key |
| Sepolia testnet | Mainnet, Polygon, Base |
| JWT sessions | Session-less stateless API |
| Credential revocation via registry | Status List 2021 |
