# Tasks: Phase 1 — Crypto Foundations

> SovereignID · openspec/changes/phase-1-crypto-foundations/tasks.md
> Estimated: 3 weeks · 5-10 hrs/week

---

## Week 1 — Keys & Signatures

### 1.1 Project setup
- [x] Create solution `SovereignID.sln`
- [x] Add `SovereignID.Crypto` class library (.NET 9)
- [x] Add `SovereignID.Crypto.Tests` xUnit project
- [x] Install Nethereum.Signer NuGet
- [x] Add `.gitignore` with secrets patterns

### 1.2 KeyPair
- [x] Implement `KeyPair.Generate()` using `EthECKey`
- [x] Implement `KeyPair.FromPrivateKey(string)`
- [x] Unit test: generated address matches expected format (0x + 40 hex)
- [x] Unit test: FromPrivateKey is deterministic

### 1.3 MessageSigner
- [x] Implement `MessageSigner.Sign()` using `EthereumMessageSigner`
- [x] Unit test: signing same message twice with same key = same signature

### 1.4 SignatureVerifier
- [x] Implement `SignatureVerifier.RecoverSigner()`
- [x] Implement `SignatureVerifier.Verify()`
- [x] Unit test: recover address matches signing address
- [x] Unit test: tampered message fails verification
- [x] Unit test: wrong address returns false

### 1.5 Week 1 deliverable
- [x] Console app prints: generated wallet, signed message, verified ✓

---

## Week 2 — Blockchain Connectivity

### 2.1 Chain project setup
- [x] Add `SovereignID.Chain` class library
- [x] Install Nethereum.Web3 NuGet
- [x] Add `appsettings.Development.json` with `SEPOLIA_RPC_URL` placeholder
- [x] Register free RPC on Infura or Alchemy (free tier, no credit card)

### 2.2 SepoliaClient
- [x] Implement `SepoliaClient` wrapping `Web3`
- [x] Implement `GetBalanceEtherAsync(address)`
- [x] Implement `GetBlockNumberAsync()`
- [x] Request testnet ETH from sepoliafaucet.com

### 2.3 Integration test
- [x] Test reads real balance from Sepolia (marked `[Trait("Category","Integration")]`)
- [x] Test reads current block number

### 2.4 Week 2 deliverable
- [x] Console app shows: Sepolia block number + test wallet balance

---

## Week 3 — Document Notarization

### 3.1 Notarizer smart contract
- [x] Write minimal Solidity contract `Notary.sol`:
  ```solidity
  mapping(bytes32 => uint256) public timestamps;
  function notarize(bytes32 hash) external { timestamps[hash] = block.timestamp; }
  ```
- [x] Compile with Hardhat or Remix (no full Hardhat setup needed)
- [x] Deploy to Sepolia, save contract address in config

### 3.2 DocumentNotarizer
- [x] Implement `HashHelper.Sha256(string content)` → bytes32
- [x] Implement `NotarizeAsync()` — calls contract, returns txHash
- [x] Implement `VerifyAsync()` — reads timestamp from contract, compares hash

### 3.3 End-to-end test
- [x] Notarize "Hello World v1" → get txHash
- [x] Verify "Hello World v1" with txHash → true
- [x] Verify "Hello World v2" with same txHash → false

### 3.4 Week 3 deliverable — Demo publishable #1
- [x] Full console demo showing all three capabilities
- [x] README.md with setup instructions and demo output
- [ ] Push to GitHub with green CI (dotnet test passing)

---

## Definition of Done

- [x] All unit tests passing (`dotnet test`)
- [x] No private keys or RPC URLs in source code
- [x] README explains how to run the demo
- [x] Demo output matches the design spec target
