using SovereignID.Auth.Application.Verify;
using SovereignID.Auth.Domain;
using SovereignID.Auth.Domain.Ports;
using SovereignID.SharedKernel.Domain;
using DomainNonce = SovereignID.Auth.Domain.Nonce;

namespace SovereignID.Auth.Application.Tests;

public class VerifySiweCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_HappyPath_ReturnsJwtAndAddress()
    {
        var fixture = await TestFixture.HappyPathAsync();
        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token", result.Value?.Jwt);
        Assert.Equal(fixture.RecoveredAddress.Value, result.Value?.Address);
    }

    [Fact]
    public async Task HandleAsync_ParseFailure_ReturnsSiweParseFailed()
    {
        var fixture = await TestFixture.HappyPathAsync();
        fixture.Parser.ParseFailure = AuthErrors.SiweParseFailed("bad parse");

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("siwe_parse_failed", result.Error?.Code);
        Assert.Equal("bad parse", result.Error?.Detail);
    }

    [Fact]
    public async Task HandleAsync_UnknownNonce_ReturnsNonceUnknown()
    {
        var fixture = await TestFixture.HappyPathAsync();
        fixture.Repository.StoredChallenge = null;

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("nonce_unknown", result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_ExpiredChallenge_ReturnsNonceExpired()
    {
        var fixture = await TestFixture.HappyPathAsync();
        fixture.Clock.Now = fixture.StoredChallenge.ExpiresAt.AddMinutes(1);

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("nonce_expired", result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_RecoveredAddressDiffers_ReturnsSignatureMismatch()
    {
        var fixture = await TestFixture.HappyPathAsync();
        fixture.SignatureVerifier.Recovered = EthereumAddress.Create("0x0000000000000000000000000000000000000001");

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("signature_mismatch", result.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_RecoveredAddressDiffers_DoesNotConsumeNonce()
    {
        var fixture = await TestFixture.HappyPathAsync();
        fixture.SignatureVerifier.Recovered = EthereumAddress.Create("0x0000000000000000000000000000000000000001");

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("signature_mismatch", result.Error?.Code);
        Assert.False(fixture.StoredChallenge.IsConsumed);
        Assert.Equal(0, fixture.Repository.DeleteCalls);
    }

    [Fact]
    public async Task HandleAsync_OnSuccess_DeletesNonceExactlyOnce()
    {
        var fixture = await TestFixture.HappyPathAsync();

        var result = await fixture.Handler.HandleAsync(fixture.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, fixture.Repository.DeleteCalls);
        Assert.Equal(fixture.Nonce, fixture.Repository.LastDeletedNonce);
    }

    private sealed class TestFixture
    {
        public VerifySiweCommandHandler Handler { get; private init; } = null!;
        public VerifySiweCommand Command { get; private init; } = null!;
        public FakeParser Parser { get; private init; } = null!;
        public FakeRepository Repository { get; private init; } = null!;
        public FakeClock Clock { get; private init; } = null!;
        public FakeSignatureVerifier SignatureVerifier { get; private init; } = null!;
        public DomainNonce Nonce { get; private init; } = null!;
        public AuthChallenge StoredChallenge { get; private init; } = null!;
        public EthereumAddress RecoveredAddress { get; private init; } = null!;

        public static async Task<TestFixture> HappyPathAsync()
        {
            var nonce = DomainNonce.Create("0123456789abcdef0123456789abcdef");
            var address = EthereumAddress.Create("0xA0b86991c6218b36c1d19d4a2e9eb0ce3606eb48");
            var issuedAt = new DateTimeOffset(2026, 4, 25, 10, 0, 0, TimeSpan.Zero);
            var challenge = await AuthChallenge.IssueAsync(
                new FakeNonceGenerator(nonce),
                new FakeClock(issuedAt),
                TimeSpan.FromMinutes(10),
                CancellationToken.None);

            var parser = new FakeParser(new SiweMessage(
                Domain: "localhost",
                Address: address,
                Statement: "Sign in",
                Uri: new Uri("http://localhost"),
                Version: 1,
                ChainId: 11155111,
                Nonce: nonce,
                IssuedAt: issuedAt,
                ExpirationTime: null,
                NotBefore: null,
                RequestId: null,
                Resources: [],
                OriginalPayload: "payload"));
            var repository = new FakeRepository(challenge);
            var clock = new FakeClock(issuedAt.AddMinutes(1));
            var signatureVerifier = new FakeSignatureVerifier(address);
            var jwtIssuer = new FakeJwtTokenIssuer(new JwtToken("jwt-token", issuedAt.AddHours(24)));
            var handler = new VerifySiweCommandHandler(parser, repository, clock, signatureVerifier, jwtIssuer);

            return new TestFixture
            {
                Handler = handler,
                Command = new VerifySiweCommand("payload", "0x1234"),
                Parser = parser,
                Repository = repository,
                Clock = clock,
                SignatureVerifier = signatureVerifier,
                Nonce = nonce,
                StoredChallenge = challenge,
                RecoveredAddress = address
            };
        }
    }

    private sealed class FakeParser(SiweMessage parsed) : ISiweMessageParser
    {
        public AuthError? ParseFailure { get; set; }

        public Task<Result<SiweMessage, AuthError>> ParseAsync(string payload, CancellationToken cancellationToken)
        {
            if (ParseFailure is not null)
            {
                return Task.FromResult(Result<SiweMessage, AuthError>.Failure(ParseFailure));
            }

            return Task.FromResult(Result<SiweMessage, AuthError>.Success(parsed));
        }
    }

    private sealed class FakeRepository(AuthChallenge challenge) : IAuthChallengeRepository
    {
        public AuthChallenge? StoredChallenge { get; set; } = challenge;
        public int DeleteCalls { get; private set; }
        public DomainNonce? LastDeletedNonce { get; private set; }

        public Task SaveAsync(AuthChallenge challenge, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<AuthChallenge?> FindByNonceAsync(DomainNonce nonce, CancellationToken cancellationToken)
            => Task.FromResult(StoredChallenge);

        public Task DeleteAsync(DomainNonce nonce, CancellationToken cancellationToken)
        {
            DeleteCalls++;
            LastDeletedNonce = nonce;
            StoredChallenge = null;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset Now { get; set; } = now;
        public Task<DateTimeOffset> GetUtcNowAsync(CancellationToken cancellationToken) => Task.FromResult(Now);
    }

    private sealed class FakeSignatureVerifier(EthereumAddress recovered) : ISiweSignatureVerifier
    {
        public EthereumAddress Recovered { get; set; } = recovered;

        public Task<EthereumAddress> RecoverAddressAsync(string message, Signature signature, CancellationToken cancellationToken)
            => Task.FromResult(Recovered);
    }

    private sealed class FakeJwtTokenIssuer(JwtToken token) : IJwtTokenIssuer
    {
        public Task<JwtToken> IssueAsync(EthereumAddress subject, TimeSpan lifetime, CancellationToken cancellationToken)
            => Task.FromResult(token);
    }

    private sealed class FakeNonceGenerator(DomainNonce nonce) : INonceGenerator
    {
        public Task<DomainNonce> NewAsync(CancellationToken cancellationToken) => Task.FromResult(nonce);
    }
}
