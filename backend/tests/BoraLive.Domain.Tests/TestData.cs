using BoraLive.Domain.Art;
using BoraLive.Domain.Games;
using BoraLive.Domain.Templates;

namespace BoraLive.Domain.Tests;

/// <summary>
/// Objetos válidos prontos, para cada teste montar só o que interessa a ele.
/// </summary>
internal static class TestData
{
    public static Game Cs2() => Game.Create("cs2", "Counter-Strike 2").Value;

    public static TextLayer Layer(TextRole role = TextRole.StreamerName) =>
        TextLayer.Create(role, x: 0.5, y: 0.5, fontSize: 0.1, color: "#FFD166").Value;

    /// <summary>Modelo de CS2 que aceita só os formatos informados (por padrão, thumbnail e banner).</summary>
    public static GameTemplate Template(params ArtFormat[] formats) =>
        GameTemplate.Create(
            "cs2-neon",
            "CS2 Neon",
            Cs2(),
            formats.Length > 0 ? formats : [ArtFormat.Thumbnail, ArtFormat.Banner],
            [Layer(TextRole.StreamerName), Layer(TextRole.LiveTitle)]).Value;
}
