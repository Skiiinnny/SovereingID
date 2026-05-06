namespace SovereignID.VerifiableCredential.Document;

/// <summary>
/// Modo de validación del documento VC respecto al bloque <c>proof</c>.
/// </summary>
public enum CredentialValidationMode
{
    /// <summary>Documento sin <c>proof</c> (emisión antes de firmar).</summary>
    Unsigned,

    /// <summary>Documento con <c>proof</c> mínimo (verificación de presentación).</summary>
    Signed,
}
