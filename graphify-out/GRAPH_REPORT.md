# Graph Report - sovereign-id-openspec  (2026-05-03)

## Corpus Check
- 217 files · ~67,858 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 568 nodes · 675 edges · 42 communities detected
- Extraction: 76% EXTRACTED · 24% INFERRED · 0% AMBIGUOUS · INFERRED: 165 edges (avg confidence: 0.8)
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
- [[_COMMUNITY_Community 16|Community 16]]
- [[_COMMUNITY_Community 17|Community 17]]
- [[_COMMUNITY_Community 18|Community 18]]
- [[_COMMUNITY_Community 19|Community 19]]
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
- [[_COMMUNITY_Community 33|Community 33]]
- [[_COMMUNITY_Community 34|Community 34]]
- [[_COMMUNITY_Community 35|Community 35]]
- [[_COMMUNITY_Community 36|Community 36]]
- [[_COMMUNITY_Community 37|Community 37]]
- [[_COMMUNITY_Community 38|Community 38]]
- [[_COMMUNITY_Community 39|Community 39]]
- [[_COMMUNITY_Community 40|Community 40]]
- [[_COMMUNITY_Community 41|Community 41]]
- [[_COMMUNITY_Community 42|Community 42]]

## God Nodes (most connected - your core abstractions)
1. `Create()` - 29 edges
2. `TituloGraduacionVcDocumentValidatorTests` - 26 edges
3. `AuthEndpointsTests` - 16 edges
4. `ArchitectureRulesTests` - 14 edges
5. `PresentationVerifierTests` - 11 edges
6. `TituloGraduacionVcDocumentValidator` - 10 edges
7. `Generate()` - 9 edges
8. `VerifySiweCommandHandlerTests` - 8 edges
9. `ManualSiweMessageParserTests` - 8 edges
10. `AuthErrors` - 7 edges

## Surprising Connections (you probably didn't know these)
- `TestClock` --inherits--> `IClock`  [EXTRACTED]
  tests\bc-auth\SovereignID.Auth.IntegrationTests\TestClock.cs →   _Bridges community 6 → community 1_
- `FakeRepository` --inherits--> `IAuthChallengeRepository`  [EXTRACTED]
  tests\bc-auth\SovereignID.Auth.Application.Tests\VerifySiweCommandHandlerTests.cs →   _Bridges community 1 → community 2_

## Communities

### Community 0 - "Community 0"
Cohesion: 0.07
Nodes (11): AuthEndpoints, ISiweSignatureVerifier, NethereumSiweSignatureVerifier, GenerateNonceQueryHandlerTests, FakeSignatureVerifier, VerifySiweCommandHandlerTests, ChainIdTests, NonceTests (+3 more)

### Community 1 - "Community 1"
Cohesion: 0.06
Nodes (14): BackgroundService, IAuthChallengeRepository, ICommandHandler, IGuidGenerator, GuidGenerator, VerifyPresentationCommandHandler, AuthChallengeEvictionHostedService, InMemoryAuthChallengeRepository (+6 more)

### Community 2 - "Community 2"
Cohesion: 0.1
Nodes (9): IQueryHandler, GenerateNonceQueryHandler, FakeRepository, AuthErrors, Create(), Failure(), Success(), AuthChallengeTests (+1 more)

### Community 3 - "Community 3"
Cohesion: 0.1
Nodes (6): PresentationVerifier, VerifierTituloGraduacionVcDocumentErrorMapper, EthrSepoliaDidParser, EthrSepoliaDidParserTests, TituloGraduacionVcDocumentValidator, VerifiablePresentationTypedData

### Community 4 - "Community 4"
Cohesion: 0.07
Nodes (11): DocumentNotarizer, DocumentNotarizerIntegrationTests, HashHelper, HashHelperTests, IBlockchainAnchor, IBlockchainQuery, IClock, IGuidGenerator (+3 more)

### Community 5 - "Community 5"
Cohesion: 0.15
Nodes (2): TituloGraduacionVcDocumentValidatorTests, TituloGraduacionVcDocumentConstants

### Community 6 - "Community 6"
Cohesion: 0.07
Nodes (13): IClock, SystemClock, INonceGenerator, SecureRandomNonceGenerator, FakeClock, FakeNonceGenerator, FakeClock, FakeNonceGenerator (+5 more)

### Community 7 - "Community 7"
Cohesion: 0.28
Nodes (4): IClassFixture, IIssuerVcIntegritySigner, AuthEndpointsTests, NethereumIssuerVcIntegritySigner

### Community 8 - "Community 8"
Cohesion: 0.24
Nodes (3): SignatureVerifier, SignatureVerifierTests, PresentationVerifierTests

### Community 9 - "Community 9"
Cohesion: 0.18
Nodes (1): ArchitectureRulesTests

### Community 10 - "Community 10"
Cohesion: 0.2
Nodes (7): IJwtTokenIssuer, ISiweMessageParser, ManualSiweMessageParser, OptionalFieldsState, FakeJwtTokenIssuer, FakeParser, TestFixture

### Community 11 - "Community 11"
Cohesion: 0.26
Nodes (5): FromEthECKey(), FromPrivateKey(), Generate(), KeyPairTests, MessageSignerTests

### Community 12 - "Community 12"
Cohesion: 0.44
Nodes (1): ManualSiweMessageParserTests

### Community 13 - "Community 13"
Cohesion: 0.28
Nodes (4): Create(), Hex32Regex(), ToString(), SecureRandomNonceGeneratorTests

### Community 14 - "Community 14"
Cohesion: 0.29
Nodes (2): SepoliaClient, SepoliaClientIntegrationTests

### Community 15 - "Community 15"
Cohesion: 0.6
Nodes (5): buildSiweMessage(), randomNonce32(), setStatus(), signIn(), toIsoUtc()

### Community 16 - "Community 16"
Cohesion: 0.33
Nodes (4): ICommand, ICommandHandler, IQuery, IQueryHandler

### Community 17 - "Community 17"
Cohesion: 0.53
Nodes (1): TituloGraduacionVcTypedData

### Community 18 - "Community 18"
Cohesion: 0.4
Nodes (1): IAuthChallengeRepository

### Community 19 - "Community 19"
Cohesion: 0.5
Nodes (2): AuthApiFactory, WebApplicationFactory

### Community 21 - "Community 21"
Cohesion: 0.67
Nodes (2): Exception, AuthDomainException

### Community 22 - "Community 22"
Cohesion: 0.67
Nodes (1): INonceGenerator

### Community 23 - "Community 23"
Cohesion: 0.67
Nodes (1): ISiweMessageParser

### Community 24 - "Community 24"
Cohesion: 0.67
Nodes (1): ISiweSignatureVerifier

### Community 25 - "Community 25"
Cohesion: 0.67
Nodes (1): IssuerTituloGraduacionDocumentErrorMapper

### Community 26 - "Community 26"
Cohesion: 0.67
Nodes (1): IssuerVcIntegritySignRequestMapper

### Community 27 - "Community 27"
Cohesion: 0.67
Nodes (1): IIssuerVcIntegritySigner

### Community 28 - "Community 28"
Cohesion: 0.67
Nodes (1): MessageSigner

### Community 29 - "Community 29"
Cohesion: 1.0
Nodes (1): Program

### Community 30 - "Community 30"
Cohesion: 1.0
Nodes (1): AuthApplicationMarker

### Community 31 - "Community 31"
Cohesion: 1.0
Nodes (1): AuthDomainMarker

### Community 32 - "Community 32"
Cohesion: 1.0
Nodes (1): AuthInfrastructureMarker

### Community 33 - "Community 33"
Cohesion: 1.0
Nodes (1): IssuerApplicationMarker

### Community 34 - "Community 34"
Cohesion: 1.0
Nodes (1): IssuerDomainMarker

### Community 35 - "Community 35"
Cohesion: 1.0
Nodes (1): IssuerInfrastructureMarker

### Community 36 - "Community 36"
Cohesion: 1.0
Nodes (1): VerifierApplicationMarker

### Community 37 - "Community 37"
Cohesion: 1.0
Nodes (1): VerifierDomainMarker

### Community 38 - "Community 38"
Cohesion: 1.0
Nodes (1): VerifierInfrastructureMarker

### Community 39 - "Community 39"
Cohesion: 1.0
Nodes (1): KernelInfrastructureMarker

### Community 40 - "Community 40"
Cohesion: 1.0
Nodes (1): TituloGraduacionVcDocumentErrorCodes

### Community 41 - "Community 41"
Cohesion: 1.0
Nodes (1): VcSliceAEip712Constants

### Community 42 - "Community 42"
Cohesion: 1.0
Nodes (1): VerifiablePresentation712

## Knowledge Gaps
- **17 isolated node(s):** `Program`, `AuthApplicationMarker`, `AuthDomainMarker`, `AuthInfrastructureMarker`, `OptionalFieldsState` (+12 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Community 5`** (30 nodes): `TituloGraduacionVcDocumentValidatorTests`, `.BuildValidSignedRoot()`, `.BuildValidUnsignedRoot()`, `.ContextNegativeCases()`, `.ProofSignedNegativeCases()`, `.SubjectNegativeCases()`, `.TypeIdIssuerIssuanceNegativeCases()`, `.Validate()`, `.Validate_accepts_leap_year_award_date()`, `.Validate_rejects_award_date_with_exact_code()`, `.Validate_rejects_expired_when_expiration_before_now()`, `.Validate_rejects_extra_term_in_second_context()`, `.Validate_rejects_invalid_subject_did()`, `.Validate_rejects_wrong_second_context_iri()`, `.Validate_root_not_object_returns_code()`, `.Validate_signed_ok_with_proof()`, `.Validate_signed_proof_branch()`, `.Validate_signed_requires_proof()`, `.Validate_unsigned_context_branch()`, `.Validate_unsigned_expiration_date_bad_format_when_present()`, `.Validate_unsigned_golden_ok()`, `.Validate_unsigned_rejects_proof_present()`, `.Validate_unsigned_subject_branch()`, `.Validate_unsigned_type_id_issuer_issuance_branch()`, `.ValidateSigned()`, `.ValidateUnsigned()`, `TituloGraduacionVcDocumentConstants`, `.CreateSecondContextObject()`, `TituloGraduacionVcDocumentConstants.cs`, `TituloGraduacionVcDocumentValidatorTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 9`** (15 nodes): `ArchitectureRulesTests`, `.Application_Layer_Has_No_Reference_To_Infrastructure()`, `.AssertOnlyAllowedProjectReferences()`, `.BC_Domain_Only_References_SharedKernel_Domain()`, `.BCs_Cannot_Reference_Each_Other()`, `.FindRepositoryRoot()`, `.GetAssembly()`, `.Issuer_Domain_References_SharedKernel_And_VcSliceA_Document()`, `.LoadAssemblies()`, `.LoadProjectReferences()`, `.Nethereum_Is_Confined_To_Infrastructure_And_Legacy()`, `.No_Project_Outside_Legacy_References_Legacy()`, `.No_Project_References_MediatR_Package()`, `.SharedKernel_Domain_Has_No_SovereignID_Dependencies()`, `ArchitectureRulesTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 12`** (10 nodes): `.ParseAsync()`, `ManualSiweMessageParserTests`, `.CanonicalPayload()`, `.Parse_CanonicalEip4361Example_ParsesAllFields()`, `.Parse_MalformedAddress_ReturnsSiweParseFailed()`, `.Parse_MissingChainId_ReturnsSiweParseFailed()`, `.Parse_ResourcesWith0Or1Or2Entries_ParsesCorrectly()`, `.Parse_UnknownChainId_ReturnsMessageWithoutPolicyRejection()`, `.Parse_VersionNotOne_ReturnsSiweParseFailed()`, `ManualSiweMessageParserTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 14`** (8 nodes): `SepoliaClient`, `.GetBalanceEtherAsync()`, `.GetBlockNumberAsync()`, `SepoliaClientIntegrationTests`, `.GetBalanceEtherAsync_ForWellKnownAddress_IsNonNegative()`, `.GetBlockNumberAsync_ReturnsPositive()`, `SepoliaClient.cs`, `SepoliaClientIntegrationTests.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 17`** (6 nodes): `TituloGraduacionVcTypedData`, `.CreateTypedData()`, `.NormalizeAddress()`, `.RecoverSignerAddress()`, `.Sign()`, `TituloGraduacionVcTypedData.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 18`** (5 nodes): `IAuthChallengeRepository`, `.DeleteAsync()`, `.FindByNonceAsync()`, `.SaveAsync()`, `IAuthChallengeRepository.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 19`** (4 nodes): `AuthApiFactory`, `.ConfigureWebHost()`, `AuthApiFactory.cs`, `WebApplicationFactory`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 21`** (3 nodes): `Exception`, `AuthDomainException`, `AuthDomainException.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 22`** (3 nodes): `INonceGenerator`, `.NewAsync()`, `INonceGenerator.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 23`** (3 nodes): `ISiweMessageParser`, `.ParseAsync()`, `ISiweMessageParser.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 24`** (3 nodes): `ISiweSignatureVerifier`, `.RecoverAddressAsync()`, `ISiweSignatureVerifier.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 25`** (3 nodes): `IssuerTituloGraduacionDocumentErrorMapper.cs`, `IssuerTituloGraduacionDocumentErrorMapper`, `.Map()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 26`** (3 nodes): `IssuerVcIntegritySignRequestMapper.cs`, `IssuerVcIntegritySignRequestMapper`, `.ToEip712()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 27`** (3 nodes): `IIssuerVcIntegritySigner.cs`, `IIssuerVcIntegritySigner`, `.SignAsync()`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 28`** (3 nodes): `MessageSigner`, `.Sign()`, `MessageSigner.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 29`** (2 nodes): `Program`, `Program.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 30`** (2 nodes): `AuthApplicationMarker`, `AuthApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 31`** (2 nodes): `AuthDomainMarker`, `AuthDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 32`** (2 nodes): `AuthInfrastructureMarker`, `AuthInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 33`** (2 nodes): `IssuerApplicationMarker`, `IssuerApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 34`** (2 nodes): `IssuerDomainMarker`, `IssuerDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 35`** (2 nodes): `IssuerInfrastructureMarker`, `IssuerInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 36`** (2 nodes): `VerifierApplicationMarker`, `VerifierApplicationMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 37`** (2 nodes): `VerifierDomainMarker`, `VerifierDomainMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 38`** (2 nodes): `VerifierInfrastructureMarker`, `VerifierInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 39`** (2 nodes): `KernelInfrastructureMarker`, `KernelInfrastructureMarker.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 40`** (2 nodes): `TituloGraduacionVcDocumentErrorCodes`, `TituloGraduacionVcDocumentErrorCodes.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 41`** (2 nodes): `VcSliceAEip712Constants`, `VcSliceAEip712Constants.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Community 42`** (2 nodes): `VerifiablePresentation712`, `VerifiablePresentation712.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Create()` connect `Community 0` to `Community 2`, `Community 3`, `Community 6`, `Community 10`, `Community 13`?**
  _High betweenness centrality (0.126) - this node is a cross-community bridge._
- **Are the 28 inferred relationships involving `Create()` (e.g. with `.HandleAsync()` and `.NewAsync()`) actually correct?**
  _`Create()` has 28 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Program`, `AuthApplicationMarker`, `AuthDomainMarker` to the rest of the system?**
  _17 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Community 0` be split into smaller, more focused modules?**
  _Cohesion score 0.07 - nodes in this community are weakly interconnected._
- **Should `Community 1` be split into smaller, more focused modules?**
  _Cohesion score 0.06 - nodes in this community are weakly interconnected._
- **Should `Community 2` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._
- **Should `Community 3` be split into smaller, more focused modules?**
  _Cohesion score 0.1 - nodes in this community are weakly interconnected._