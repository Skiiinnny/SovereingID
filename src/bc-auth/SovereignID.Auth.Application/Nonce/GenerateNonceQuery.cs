using SovereignID.SharedKernel.Application;

namespace SovereignID.Auth.Application.Nonce;

public sealed record GenerateNonceQuery : IQuery<GenerateNonceResult>;
