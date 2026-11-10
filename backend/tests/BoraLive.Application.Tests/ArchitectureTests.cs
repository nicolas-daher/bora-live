using System.Reflection;

namespace BoraLive.Application.Tests;

// Protege a regra de dependência da Clean Architecture:
// a Application só pode conhecer o Domain.
public class ArchitectureTests
{
    private static readonly string[] ForbiddenPrefixes =
    [
        "BoraLive.Infrastructure",
        "BoraLive.Api",
        "Microsoft.AspNetCore", // sem framework web
        "SkiaSharp",            // sem biblioteca de imagem
    ];

    [Fact]
    public void Application_DependsOnlyOnDomain()
    {
        var application = Assembly.Load("BoraLive.Application");

        var forbidden = application.GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => ForbiddenPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.Ordinal)))
            .ToList();

        Assert.Empty(forbidden);
    }
}
