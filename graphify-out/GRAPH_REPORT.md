# Graph Report - sovereign-id-openspec  (2026-05-03)

## Corpus Check
- 171 files · ~52,553 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 432 nodes · 468 edges · 32 communities detected
- Extraction: 75% EXTRACTED · 25% INFERRED · 0% AMBIGUOUS · INFERRED: 117 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Community 0|Community 0]]
- [[_COMMUNITY_Community 1|Community 1]]
- [[_COMMUNITY_Community 2|Community 2]]
- [[_COMMUNITY_Community 3|Community 3]]
- [[_COMMUNITY_Community 4|Community 4]]
- [[_COMMUNITY_Community 5|Community 5]]
- [[_COMMUNITY_Community 6|Community 6]]
- [[_COMMUNITY_Community 7|Community 7]]
- [[_COMMUNITY_Community 8|Community 8]]
- [[_COMMUNITY_Community 9|Community 9]]
- [[_COMMUNITY_Community 10|Community 10]]
- [[_COMMUNITY_Community 11|Community 11]]
- [[_COMMUNITY_Community 12|Community 12]]
- [[_COMMUNITY_Community 13|Community 13]]
- [[_COMMUNITY_Community 14|Community 14]]
- [[_COMMUNITY_Community 15|Community 15]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
- [[_COMMUNITY_Community 20|Community 20]]
- [[_COMMUNITY_Community 21|Community 21]]
- [[_COMMUNITY_Community 22|Community 22]]
- [[_COMMUNITY_Community 23|Community 23]]
- [[_COMMUNITY_Community 24|Community 24]]
- [[_COMMUNITY_Community 25|Community 25]]
- [[_COMMUNITY_Community 26|Community 26]]
- [[_COMMUNITY_Community 27|Community 27]]
- [[_COMMUNITY_Community 28|Community 28]]
- [[_COMMUNITY_Community 29|Community 29]]
- [[_COMMUNITY_Community 30|Community 30]]
- [[_COMMUNITY_Community 31|Community 31]]
- [[_COMMUNITY_Community 32|Community 32]]

## God Nodes (most connected - your core abstractions)
1. `Create()` - 28 edges
2. `AuthEndpointsTests` - 16 edges
3. `ArchitectureRulesTests` - 13 edges
4. `Generate()` - 9 edges
5. `VerifySiweCommandHandlerTests` - 8 edges
6. `ManualSiweMessageParserTests` - 8 edges
7. `AuthErrors` - 7 edges
8. `ManualSiweMessageParser` - 7 edges
9. `ValueObjectsTests` - 7 edges
10. `InMemoryAuthChallengeRepository` - 6 edges

## Surprising Connections (you probably didn't know these)
- `TestClock` --inherits--> `IClock`  [EXTRACTED]
  tests\bc-auth\SovereignID.Auth.IntegrationTests\TestClock.cs →   _Bridges community 2 → community 6_
- `FakeRepository` --inherits--> `IAuthChallengeRepository`  [EXTRACTED]
  tests\bc-auth\SovereignID.Auth.Application.Tests\VerifySiweCommandHandlerTests.cs →   _Bridges community 2 → community 1_
- `ManualSiweMessageParser` --inherits--> `ISiweMessageParser`  [EXTRACTED]
  src\bc-auth\SovereignID.Auth.Infrastructure\Siwe\ManualSiweMessageParser.cs →   _Bridges community 10 → community 7_

## Communities

### Community 0 - "Community 0"
Cohesion: 0.07
Nodes (12): ISiweSignatureVerifier, NethereumSiweSignatureVerifier, FakeSignatureVerifier, Create(), Hex32Regex(), ToString(), ChainIdTests, NonceTests (+4 more)

### Community 1 - "Community 1"
Cohesion: 0.09
Nodes (11): ICommandHandler, IQueryHandler, GenerateNonceQueryHandler, FakeRepository, AuthErrors, Create(), Failure(), Success() (+3 more)

### Community 2 - "Community 2"
Cohesion: 0.06
Nodes (15): IAuthChallengeRepository, IClock, SystemClock, INonceGenerator, SecureRandomNonceGenerator, FakeClock, FakeNonceGenerator, SpyRepository (+7 more)

### Community 3 - "Community 3"
Cohesion: 0.07
Nodes (11): DocumentNotarizer, DocumentNotarizerIntegrationTests, HashHelper, HashHelperTests, IBlockchainAnchor, IBlockchainQuery, IClock, IGuidGenerator (+3 more)

### Community 4 - "Community 4"
Cohesion: 0.21
Nodes (4): IClassFixture, AuthEndpointsTests, SignatureVerifier, SignatureVerifierTests

### Community 5 - "Community 5"
Cohesion: 0.13
Nodes (7): AuthEndpoints, GenerateNonceQueryHandlerTests, VerifySiweCommandHandlerTests, ICommand, ICommandHandler, IQuery, IQueryHandler

### Community 6 - "Community 6"
Cohesion: 0.12
Nodes (5): BackgroundService, AuthChallengeEvictionHostedService, InMemoryAuthChallengeRepository, AuthChallenge, TestClock

### Community 7 - "Community 7"
Cohesion: 0.22
Nodes (6): IJwtTokenIssuer, ISiweMessageParser, FakeJwtTokenIssuer, FakeParser, TestFixture, ManualSiweMessageParserTests

### Community 8 - "Community 8"
Cohesion: 0.19
Nodes (1): ArchitectureRulesTests

### Community 9 - "Community 9"
Cohesion: 0.26
Nodes (5): FromEthECKey(), FromPrivateKey(), Generate(), KeyPairTests, MessageSignerTests

### Community 10 - "Community 10"
Cohesion: 0.43
Nodes (2): ManualSiweMessageParser, OptionalFieldsState

### Community 11 - "Community 11"
Cohesion: 0.29
Nodes (2): SepoliaClient, SepoliaClientIntegrationTests

### Community 12 - "Community 12"
Cohesion: 0.6
Nodes (5): buildSiweMessage(), randomNonce32(), setStatus(), signIn(), toIsoUtc()

### Community 13 - "Community 13"
Cohesion: 0.4
Nodes (1): IAuthChallengeRepository

### Community 14 - "Community 14"
Cohesion: 0.5
Nodes (2): IGuidGenerator, GuidGenerator

### Community 15 - "Community 15"
Cohesion: 0.5
Nodes (2): AuthApiFactory, WebApplicationFactory

### Community 17 - "Community 17"
Cohesion: 0.67
Nodes (2): Exception, AuthDomainException

### Community 18 - "Community 18"
Cohesion: 0.67
Nodes (1): INonceGenerator

### Community 19 - "Community 19"
Cohesion: 0.67
Nodes (1): ISiweMessageParser

### Community 20 - "Community 20"
Cohesion: 0.67
Nodes (1): ISiweSignatureVerifier

### Community 21 - "Community 21"
Cohesion: 0.67
Nodes (1): MessageSigner

### Community 22 - "Community 22"
Cohesion: 1.0
Nodes (1): Program

### Community 23 - "Community 23"
Cohesion: 1.0
Nodes (1): AuthApplicationMarker

### Community 24 - "Community 24"
Cohesion: 1.0
Nodes (1): AuthDomainMarker

### Community 25 - "Community 25"
Cohesion: 1.0
Nodes (1): AuthInfrastructureMarker

### Community 26 - "Community 26"
Cohesion: 1.0
Nodes (1): IssuerApplicationMarker

### Community 27 - "Community 27"
Cohesion: 1.0
Nodes (1): IssuerDomainMarker

### Community 28 - "Community 28"
Cohesion: 1.0
Nodes (1): IssuerInfrastructureMarker

### Community 29 - "Community 29"
Cohesion: 1.0
Nodes (1): VerifierApplicationMarker

### Community 30 - "Community 30"
Cohesion: 1.0
Nodes (1): VerifierDomainMarker

### Community 31 - "Community 31"
Cohesion: 1.0
Nodes (1): VerifierInfrastructureMarker

### Community 32 - "Community 32"
Cohesion: 1.0
Nodes (1): KernelInfrastructureMarker

## Knowledge Gaps
- **14 isolated node(s):** `Program`, `AuthApplicationMarker`, `AuthDomainMarker`, `AuthInfrastructureMarker`, `OptionalFieldsState` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 8`** (14 nodes): `ArchitectureRulesTests`, `.Application_Layer_Has_No_Reference_To_Infrastructure()`, `.AssertOnlyAllowedProjectReferences()`, `.BC_Domain_Only_References_SharedKernel_Domain()`, `.BCs_Cannot_Reference_Each_Other()`, `.FindRepositoryRoot()`, `.GetAssembly()`, `.LoadAssemblies()`, `.LoadProjectReferences()`, `.Nethereum_Is_Confined_To_Infrastructure_And_Legacy()`, `.No_Project_Outside_Legacy_References_Legacy()`, `.No_Project_References_MediatR_Package()`, `.SharedKernel_Domain_Has_No_SovereignID_Dependencies()`, `ArchitectureRulesTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 10`** (8 nodes): `ManualSiweMessageParser`, `.Ensure()`, `.IsHexAddress()`, `.ParseAsync()`, `.ParsePrefixed()`, `.TryParseOptionalLine()`, `OptionalFieldsState`, `ManualSiweMessageParser.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 11`** (8 nodes): `SepoliaClient`, `.GetBalanceEtherAsync()`, `.GetBlockNumberAsync()`, `SepoliaClientIntegrationTests`, `.GetBalanceEtherAsync_ForWellKnownAddress_IsNonNegative()`, `.GetBlockNumberAsync_ReturnsPositive()`, `SepoliaClient.cs`, `SepoliaClientIntegrationTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 13`** (5 nodes): `IAuthChallengeRepository`, `.DeleteAsync()`, `.FindByNonceAsync()`, `.SaveAsync()`, `IAuthChallengeRepository.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 14`** (4 nodes): `IGuidGenerator`, `GuidGenerator`, `.NewGuidAsync()`, `GuidGenerator.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 15`** (4 nodes): `AuthApiFactory`, `.ConfigureWebHost()`, `AuthApiFactory.cs`, `WebApplicationFactory`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 17`** (3 nodes): `Exception`, `AuthDomainException`, `AuthDomainException.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 18`** (3 nodes): `INonceGenerator`, `.NewAsync()`, `INonceGenerator.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 19`** (3 nodes): `ISiweMessageParser`, `.ParseAsync()`, `ISiweMessageParser.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 20`** (3 nodes): `ISiweSignatureVerifier`, `.RecoverAddressAsync()`, `ISiweSignatureVerifier.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 21`** (3 nodes): `MessageSigner`, `.Sign()`, `MessageSigner.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 22`** (2 nodes): `Program`, `Program.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 23`** (2 nodes): `AuthApplicationMarker`, `AuthApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 24`** (2 nodes): `AuthDomainMarker`, `AuthDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 25`** (2 nodes): `AuthInfrastructureMarker`, `AuthInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 26`** (2 nodes): `IssuerApplicationMarker`, `IssuerApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 27`** (2 nodes): `IssuerDomainMarker`, `IssuerDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 28`** (2 nodes): `IssuerInfrastructureMarker`, `IssuerInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 29`** (2 nodes): `VerifierApplicationMarker`, `VerifierApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 30`** (2 nodes): `VerifierDomainMarker`, `VerifierDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 31`** (2 nodes): `VerifierInfrastructureMarker`, `VerifierInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 32`** (2 nodes): `KernelInfrastructureMarker`, `KernelInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Create()` connect `Community 0` to `Community 1`, `Community 2`, `Community 10`, `Community 5`?**
  _High betweenness centrality (0.108) - this node is a cross-community bridge._
- **Why does `ToString()` connect `Community 0` to `Community 4`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **Why does `TestClock` connect `Community 6` to `Community 2`?**
  _High betweenness centrality (0.036) - this node is a cross-community bridge._
- **Are the 27 inferred relationships involving `Create()` (e.g. with `.HandleAsync()` and `.NewAsync()`) actually correct?**
  _`Create()` has 27 INFERRED edges - model-reasoned connections that need verification._
- **Are the 7 inferred relationships involving `Generate()` (e.g. with `.Generate_Address_Is0xPlus40Hex()` and `.FromPrivateKey_IsDeterministic()`) actually correct?**
  _`Generate()` has 7 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Program`, `AuthApplicationMarker`, `AuthDomainMarker` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._