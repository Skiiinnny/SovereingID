using System.Reflection;
using SovereignID.Verifier.Application.Presentation;
using SovereignID.VerifiableCredential.Document;

namespace SovereignID.Verifier.Application.Tests;

public sealed class VerifierTituloGraduacionVcDocumentErrorMapperTests
{
    public static IEnumerable<object[]> DocumentErrorCodeMembers() =>
        typeof(TituloGraduacionVcDocumentErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new[] { f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(DocumentErrorCodeMembers))]
    public void Map_returns_non_empty_api_code_for_each_document_code(string documentCode)
    {
        var api = VerifierTituloGraduacionVcDocumentErrorMapper.Map(documentCode);
        Assert.False(string.IsNullOrWhiteSpace(api));
    }

    [Fact]
    public void Map_unknown_code_returns_generic_invalid()
    {
        Assert.Equal("vc_document_invalid", VerifierTituloGraduacionVcDocumentErrorMapper.Map("totally_unknown_code"));
    }
}
