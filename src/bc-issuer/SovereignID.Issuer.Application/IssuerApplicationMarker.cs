using System.Diagnostics.CodeAnalysis;

namespace SovereignID.Issuer.Application;

/// <summary>
/// Public assembly anchor: this type is referenced for assembly identity, peer project
/// references, and layer architecture rules. Intentionally minimal; see the solution-architecture
/// spec (assembly anchor).
/// </summary>
[SuppressMessage("csharpsquid", "S2094", Justification = "Assembly anchor per solution-architecture; no instance members by design (architecture tests and references).")]
public sealed class IssuerApplicationMarker;
