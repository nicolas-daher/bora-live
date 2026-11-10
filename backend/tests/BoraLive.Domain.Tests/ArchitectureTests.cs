using System.Reflection;

namespace BoraLive.Domain.Tests;

// Protege a regra de dependência da Clean Architecture:
// o Domain é o núcleo e não pode depender de mais nada.
public class ArchitectureTests
{
    private static readonly string[] ForbiddenPrefixes =
    [
        "BoraLive.",            // nenhuma outra camada do projeto
        "Microsoft.AspNetCore", // sem framework web
        "SkiaSharp",            // sem biblioteca de imagem
        "System.Net.Http",      // sem chamadas HTTP
    ];

    [Fact]
    public void Domain_DoesNotDependOnOtherLayersOrFrameworks()
    {
        var domain = Assembly.Load("BoraLive.Domain");

        var forbidden = domain.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => ForbiddenPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(forbidden);
    }
}
