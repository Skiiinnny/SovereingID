namespace SovereignID.Auth.Api.Contracts;

/// <summary>
/// Request payload for SIWE verification.
/// </summary>
/// <param name="Message">Raw SIWE message payload.</param>
/// <param name="Signature">Wallet signature for the SIWE message.</param>
public sealed record VerifyRequest(string Message, string Signature);
