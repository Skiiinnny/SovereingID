using SovereignID.Auth.Api.Contracts;
using SovereignID.Auth.Application.Nonce;
using SovereignID.Auth.Application.Verify;
using SovereignID.Auth.Domain;
using SovereignID.SharedKernel.Application;

namespace SovereignID.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints used for SIWE login.
    /// </summary>
    /// <param name="app">Target web application.</param>
    public static void MapAuth(WebApplication app)
    {
        app.MapGet("/auth/nonce", async (
            IQueryHandler<GenerateNonceQuery, GenerateNonceResult> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GenerateNonceQuery(), cancellationToken);
            return Results.Ok(new NonceResponse(result.Nonce, result.ExpiresAt));
        });

        app.MapPost("/auth/verify", async (
            VerifyRequest body,
            ICommandHandler<VerifySiweCommand, Result<VerifySiweResult, AuthError>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new VerifySiweCommand(body.Message, body.Signature), cancellationToken);
            if (result.IsSuccess)
            {
                var value = result.Value!;
                return Results.Ok(new VerifyResponse(value.Jwt, value.Address, value.ExpiresAt));
            }

            return ToProblem(result.Error!);
        });
    }

    private static IResult ToProblem(AuthError error)
    {
        var statusCode = error.Code switch
        {
            "siwe_parse_failed" => StatusCodes.Status400BadRequest,
            "unsupported_chain" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status401Unauthorized
        };

        return Results.Problem(
            detail: error.Detail,
            statusCode: statusCode,
            title: "Authentication failed",
            extensions: new Dictionary<string, object?>
            {
                ["error"] = error.Code
            });
    }
}
