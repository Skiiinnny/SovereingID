# Design: Phase 1 — Crypto Foundations

> SovereignID · openspec/changes/phase-1-crypto-foundations/design.md

## Solution Structure

```
SovereignID/
├── src/
│   ├── SovereignID.Crypto/           ← Core crypto primitives
│   │   ├── KeyPair.cs
│   │   ├── MessageSigner.cs
│   │   ├── SignatureVerifier.cs
│   │   └── HashHelper.cs
│   ├── SovereignID.Chain/            ← Blockchain connectivity
│   │   ├── SepoliaClient.cs
│   │   └── DocumentNotarizer.cs
│   └── SovereignID.Demo.Phase1/      ← Runnable console demo
│       └── Program.cs
└── tests/
    └── SovereignID.Crypto.Tests/
        ├── KeyPairTests.cs
        ├── MessageSignerTests.cs
        └── SignatureVerifierTests.cs
```

## Key Classes

### KeyPair.cs

```csharp
public record KeyPair(string PrivateKey, string PublicKey, string Address)
{
    public static KeyPair Generate();
    public static KeyPair FromPrivateKey(string privateKey);
}
```

### MessageSigner.cs

```csharp
public class MessageSigner
{
    // Signs arbitrary string using Ethereum personal_sign prefix
    public string Sign(string message, string privateKey);
}
```

### SignatureVerifier.cs

```csharp
public class SignatureVerifier
{
    // Recovers signer address from message + signature
    // Returns address string — caller compares with expected
    public string RecoverSigner(string message, string signature);
    public bool Verify(string message, string signature, string expectedAddress);
}
```

### SepoliaClient.cs

```csharp
public class SepoliaClient
{
    // Wraps Web3 with Sepolia RPC URL
    public SepoliaClient(string rpcUrl);
    public Task<decimal> GetBalanceEtherAsync(string address);
    public Task<ulong> GetBlockNumberAsync();
}
```

### DocumentNotarizer.cs

```csharp
public class DocumentNotarizer
{
    // Hashes a string/file and stores on-chain
    // Returns transaction hash
    public Task<string> NotarizeAsync(string content, string privateKey);
    // Returns true if hash matches what's on-chain at that tx
    public Task<bool> VerifyAsync(string content, string txHash);
}
```

## NuGet Dependencies

```xml
<PackageReference Include="Nethereum.Web3" Version="4.x" />
<PackageReference Include="Nethereum.Signer" Version="4.x" />
<PackageReference Include="Nethereum.Contracts" Version="4.x" />
```

## Demo Output (target)

```
=== SovereignID Phase 1 Demo ===

[KeyPair]
Address : 0x71C7656EC7ab88b098defB751B7401B5f6d8976F
Public  : 0x04a1b2...
Private : *** (hidden in output)

[Sign & Verify]
Message   : "Hello SovereignID"
Signature : 0x1b3f...
Recovered : 0x71C7656EC7ab88b098defB751B7401B5f6d8976F
Match     : ✓ TRUE

[Chain — Sepolia]
Block     : 8,432,901
Balance   : 0.05 ETH

[Notarize]
Content   : "My important document v1.0"
SHA256    : 0xabc123...
TxHash    : 0xdef456...
Verified  : ✓ TRUE
```

## Constraints

- Private key NEVER logged, only address shown in demo
- RPC URL loaded from environment variable `SEPOLIA_RPC_URL`, never hardcoded
- All async — no `.Result` or `.Wait()`
