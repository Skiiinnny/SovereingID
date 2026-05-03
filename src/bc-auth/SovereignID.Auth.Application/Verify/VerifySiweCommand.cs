using SovereignID.Auth.Domain;
using SovereignID.SharedKernel.Application;

namespace SovereignID.Auth.Application.Verify;

public sealed record VerifySiweCommand(string Message, string Signature) : ICommand<Result<VerifySiweResult, AuthError>>;
