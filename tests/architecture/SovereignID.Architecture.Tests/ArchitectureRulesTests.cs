using System.Reflection;
using System.Xml.Linq;
using NetArchTest.Rules;

namespace SovereignID.Architecture.Tests;

public sealed class ArchitectureRulesTests
{
    private static readonly Lazy<Dictionary<string, Assembly>> Assemblies = new(LoadAssemblies);
    private static readonly Lazy<Dictionary<string, List<string>>> ProjectReferences = new(LoadProjectReferences);

    [Fact]
    public void SharedKernel_Domain_Has_No_SovereignID_Dependencies()
    {
        var assembly = GetAssembly("SovereignID.SharedKernel.Domain");
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn("SovereignID.")
            .GetResult();

        if (result.IsSuccessful)
        {
            return;
        }

        var failures = result.FailingTypeNames is { Count: > 0 }
            ? string.Join(", ", result.FailingTypeNames)
            : "(no type details)";
        Assert.Fail($"Rule SharedKernel.Domain independence violated. Offending assembly: {assembly.GetName().Name}. Edge: type dependency to SovereignID.*. Failing types: {failures}");
    }

    [Theory]
    [InlineData("SovereignID.Auth.Domain")]
    [InlineData("SovereignID.Verifier.Domain")]
    public void BC_Domain_Only_References_SharedKernel_Domain(string assemblyName)
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "SovereignID.SharedKernel.Domain"
        };

        AssertOnlyAllowedProjectReferences(assemblyName, "BC domain only references SharedKernel.Domain", allowed);
    }

    [Fact]
    public void Issuer_Domain_References_SharedKernel_And_VcSliceA_Document()
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "SovereignID.SharedKernel.Domain",
            "SovereignID.VcSliceA.Document",
        };

        AssertOnlyAllowedProjectReferences(
            "SovereignID.Issuer.Domain",
            "Issuer.Domain references SharedKernel.Domain and VcSliceA.Document only",
            allowed);
    }

    [Fact]
    public void Application_Layer_Has_No_Reference_To_Infrastructure()
    {
        var applicationAssemblies = Assemblies.Value.Values
            .Where(a => a.GetName().Name is { } n &&
                        n.StartsWith("SovereignID.", StringComparison.Ordinal) &&
                        n.EndsWith(".Application", StringComparison.Ordinal))
            .ToList();

        foreach (var assembly in applicationAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Infrastructure")
                .GetResult();

            if (result.IsSuccessful)
            {
                continue;
            }

            var failures = result.FailingTypeNames is { Count: > 0 }
                ? string.Join(", ", result.FailingTypeNames)
                : "(no type details)";
            Assert.Fail($"Rule Application without Infrastructure violated. Offending assembly: {assembly.GetName().Name}. Edge: type dependency matching 'Infrastructure'. Failing types: {failures}");
        }
    }

    [Fact]
    public void BCs_Cannot_Reference_Each_Other()
    {
        var bcRoots = new[] { "SovereignID.Auth.", "SovereignID.Issuer.", "SovereignID.Verifier." };
        foreach (var pair in ProjectReferences.Value)
        {
            var name = pair.Key;
            var root = bcRoots.FirstOrDefault(name.StartsWith);
            if (root is null)
            {
                continue;
            }

            var forbiddenRoots = bcRoots.Where(r => r != root).ToArray();
            foreach (var refName in pair.Value)
            {
                var forbidden = forbiddenRoots.FirstOrDefault(refName.StartsWith);
                if (forbidden is null)
                {
                    continue;
                }

                Assert.Fail($"Rule BC isolation violated. Offending assembly: {name}. Edge: {name} -> {refName}.");
            }
        }
    }

    [Fact]
    public void No_Project_Outside_Legacy_References_Legacy()
    {
        foreach (var pair in ProjectReferences.Value)
        {
            var name = pair.Key;
            if (!name.StartsWith("SovereignID.", StringComparison.Ordinal) || name.Contains(".Architecture.Tests", StringComparison.Ordinal))
            {
                continue;
            }

            var isLegacyAssembly = name is "SovereignID.Crypto" or "SovereignID.Chain" or "SovereignID.Demo.Phase1";
            if (isLegacyAssembly)
            {
                continue;
            }

            foreach (var refName in pair.Value)
            {
                if (refName is not ("SovereignID.Crypto" or "SovereignID.Chain" or "SovereignID.Demo.Phase1"))
                {
                    continue;
                }

                Assert.Fail($"Rule no-new-to-legacy violated. Offending assembly: {name}. Edge: {name} -> {refName}.");
            }
        }
    }

    [Fact]
    public void Nethereum_Is_Confined_To_Infrastructure_And_Legacy()
    {
        foreach (var assembly in Assemblies.Value.Values)
        {
            var name = assembly.GetName().Name ?? string.Empty;
            if (!name.StartsWith("SovereignID.", StringComparison.Ordinal))
            {
                continue;
            }

            var isLegacy = name is "SovereignID.Crypto" or "SovereignID.Chain" or "SovereignID.Demo.Phase1";
            var isInfrastructure = name.EndsWith(".Infrastructure", StringComparison.Ordinal);
            var isSharedEip712Contracts = name == "SovereignID.VcSliceA.Eip712";
            if (isLegacy || isInfrastructure || isSharedEip712Contracts)
            {
                continue;
            }

            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                var refName = reference.Name ?? string.Empty;
                if (!refName.StartsWith("Nethereum.", StringComparison.Ordinal))
                {
                    continue;
                }

                Assert.Fail($"Rule Nethereum confinement violated. Offending assembly: {name}. Edge: {name} -> {refName}.");
            }
        }
    }

    [Fact]
    public void No_Project_References_MediatR_Package()
    {
        var repoRoot = FindRepositoryRoot();
        var csprojFiles = Directory.GetFiles(Path.Combine(repoRoot, "src"), "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}legacy{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var csproj in csprojFiles)
        {
            var doc = XDocument.Load(csproj);
            var packageReferences = doc.Descendants("PackageReference");
            foreach (var reference in packageReferences)
            {
                var include = reference.Attribute("Include")?.Value ?? string.Empty;
                if (!include.StartsWith("MediatR", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Assert.Fail($"Rule no MediatR violated. Offending assembly file: {Path.GetFileName(csproj)}. Edge: PackageReference -> {include}.");
            }
        }
    }

    private static void AssertOnlyAllowedProjectReferences(string assemblyName, string ruleName, HashSet<string> allowed)
    {
        if (!ProjectReferences.Value.TryGetValue(assemblyName, out var references))
        {
            Assert.Fail($"Project reference discovery failed. Missing project: {assemblyName}. Rule cannot be evaluated.");
            return;
        }

        foreach (var reference in references)
        {
            if (allowed.Contains(reference))
            {
                continue;
            }

            Assert.Fail($"Rule {ruleName} violated. Offending assembly: {assemblyName}. Edge: {assemblyName} -> {reference}.");
        }
    }

    private static Assembly GetAssembly(string assemblyName)
    {
        if (Assemblies.Value.TryGetValue(assemblyName, out var assembly))
        {
            return assembly;
        }

        Assert.Fail($"Assembly discovery failed. Missing assembly: {assemblyName}. Rule cannot be evaluated.");
        throw new InvalidOperationException($"Missing assembly {assemblyName}.");
    }

    private static Dictionary<string, Assembly> LoadAssemblies()
    {
        var map = new Dictionary<string, Assembly>(StringComparer.Ordinal);
        var baseDirectory = AppContext.BaseDirectory;
        var assemblyFiles = Directory.GetFiles(baseDirectory, "SovereignID*.dll", SearchOption.TopDirectoryOnly)
            .Where(path => !Path.GetFileName(path).Equals("SovereignID.Architecture.Tests.dll", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in assemblyFiles)
        {
            var assembly = Assembly.LoadFrom(file);
            var name = assembly.GetName().Name;
            if (name is null || !name.StartsWith("SovereignID.", StringComparison.Ordinal))
            {
                continue;
            }

            map[name] = assembly;
        }

        foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = loaded.GetName().Name;
            if (name is null || !name.StartsWith("SovereignID.", StringComparison.Ordinal) || name.Contains(".Architecture.Tests", StringComparison.Ordinal))
            {
                continue;
            }

            map[name] = loaded;
        }

        return map;
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "SovereignID.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing SovereignID.sln.");
    }

    private static Dictionary<string, List<string>> LoadProjectReferences()
    {
        var repoRoot = FindRepositoryRoot();
        var srcRoot = Path.Combine(repoRoot, "src");
        var projectFiles = Directory.GetFiles(srcRoot, "*.csproj", SearchOption.AllDirectories);
        var projectByPath = projectFiles.ToDictionary(Path.GetFullPath, p => Path.GetFileNameWithoutExtension(p), StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var csproj in projectFiles)
        {
            var thisProject = Path.GetFileNameWithoutExtension(csproj);
            var doc = XDocument.Load(csproj);
            var references = new List<string>();
            foreach (var node in doc.Descendants("ProjectReference"))
            {
                var include = node.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(include))
                {
                    continue;
                }

                var targetPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csproj)!, include));
                if (projectByPath.TryGetValue(targetPath, out var targetProject))
                {
                    references.Add(targetProject);
                }
            }

            result[thisProject] = references;
        }

        return result;
    }
}
