using BoraLive.Application.Abstractions;
using BoraLive.Application.Art;
using BoraLive.Domain.Art;
using BoraLive.Domain.Common;
using BoraLive.Domain.Templates;
using SkiaSharp;

namespace BoraLive.Infrastructure.Imaging;

/// <summary>
/// Implementação da porta <see cref="IImageComposer"/> com SkiaSharp.
/// É o único lugar do sistema que sabe desenhar: fundo, logo e textos, nesta ordem.
/// </summary>
public sealed class SkiaImageComposer : IImageComposer
{
    /// <summary>Maior largura ou altura aceita para o logo, em pixels.</summary>
    private const int MaxLogoDimension = 4096;

    /// <summary>Até onde um texto pode encolher para caber: 45% do tamanho do layout.</summary>
    private const float MinFontScale = 0.45f;

    /// <summary>Distância entre linhas, em relação ao tamanho da fonte.</summary>
    private const float LineHeight = 1.15f;

    /// <summary>Maior largura ou altura aceita para o fundo gerado pela IA.</summary>
    private const int MaxBackgroundDimension = 8192;

    /// <summary>Opacidade da película escura sobre o fundo da IA (0 a 255): garante contraste para o texto.</summary>
    private const byte BackgroundScrimAlpha = 110;

    public Result<byte[]> Compose(ArtRequest request, byte[]? logo, byte[]? background)
    {
        var format = request.Format;

        // O Domain garante que o modelo aceita o formato, então o layout existe.
        var layout = request.Template.LayoutFor(format)
            ?? throw new InvalidOperationException("O pedido chegou com um formato que o modelo não aceita.");

        // "using var": o objeto é liberado ao sair do método.
        // Importante aqui, porque o SkiaSharp guarda as imagens em memória nativa, fora do controle do .NET.
        using var surface = SKSurface.Create(new SKImageInfo(format.Width, format.Height));
        var canvas = surface.Canvas;

        if (background is not null)
        {
            using var backgroundImage = DecodeImage(background, MaxBackgroundDimension);

            if (backgroundImage is null)
            {
                // Quem chamou desenha de novo sem o fundo (degradê) e avisa a pessoa.
                return ArtErrors.AiUnavailable;
            }

            DrawImageBackground(canvas, format, backgroundImage);
        }
        else
        {
            DrawBackground(canvas, format, request.Template.Background);
        }

        if (logo is not null)
        {
            using var logoImage = DecodeImage(logo, MaxLogoDimension);

            if (logoImage is null)
            {
                return ArtErrors.LogoInvalid;
            }

            if (layout.Logo is not null)
            {
                DrawLogo(canvas, format, layout.Logo, logoImage);
            }
        }

        foreach (var layer in layout.Layers)
        {
            DrawText(canvas, format, layer, TextFor(layer.Role, request));
        }

        using var image = surface.Snapshot();
        using var png = image.Encode(SKEncodedImageFormat.Png, quality: 100);

        return png.ToArray();
    }

    private static string TextFor(TextRole role, ArtRequest request) => role switch
    {
        TextRole.StreamerName => request.StreamerName,
        TextRole.LiveTitle => request.LiveTitle,
        _ => request.Template.CallToAction,
    };

    // ---------- Fundo ----------

    private static void DrawBackground(SKCanvas canvas, ArtFormat format, TemplateBackground background)
    {
        // Mesmo cálculo do "linear-gradient" do CSS: 0 grau sobe, 90 vai para a direita.
        var radians = background.Angle * Math.PI / 180;
        var directionX = (float)Math.Sin(radians);
        var directionY = (float)-Math.Cos(radians);

        // Comprimento da linha do degradê para ele alcançar os cantos da arte.
        var length = Math.Abs(format.Width * directionX) + Math.Abs(format.Height * directionY);

        var center = new SKPoint(format.Width / 2f, format.Height / 2f);
        var offset = new SKPoint(directionX * length / 2, directionY * length / 2);

        using var shader = SKShader.CreateLinearGradient(
            center - offset,
            center + offset,
            background.Colors.Select(SKColor.Parse).ToArray(),
            SKShaderTileMode.Clamp);

        using var paint = new SKPaint { Shader = shader };

        canvas.DrawRect(0, 0, format.Width, format.Height, paint);
    }

    /// <summary>
    /// Desenha a imagem da IA cobrindo a arte inteira ("cover"): escala até preencher e corta o que sobra,
    /// centralizado. Depois escurece de leve, porque a IA não sabe onde o texto vai ficar.
    /// </summary>
    private static void DrawImageBackground(SKCanvas canvas, ArtFormat format, SKImage image)
    {
        var scale = Math.Max((float)format.Width / image.Width, (float)format.Height / image.Height);

        // Parte da imagem original que cabe na arte, depois de escalada.
        var sourceWidth = format.Width / scale;
        var sourceHeight = format.Height / scale;
        var source = SKRect.Create(
            (image.Width - sourceWidth) / 2,
            (image.Height - sourceHeight) / 2,
            sourceWidth,
            sourceHeight);

        var destination = SKRect.Create(0, 0, format.Width, format.Height);
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

        canvas.DrawImage(image, source, destination, sampling);

        using var scrim = new SKPaint { Color = new SKColor(0, 0, 0, BackgroundScrimAlpha) };
        canvas.DrawRect(destination, scrim);
    }

    // ---------- Logo ----------

    /// <summary>Abre uma imagem. Devolve null se o arquivo não for uma imagem aceita.</summary>
    private static SKImage? DecodeImage(byte[] bytes, int maxDimension)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);

        // O codec lê só o cabeçalho do arquivo. Dá para recusar cedo, sem descompactar:
        // um arquivo pequeno pode declarar uma imagem gigante para esgotar a memória.
        if (codec is null
            || codec.EncodedFormat is not (SKEncodedImageFormat.Png or SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Webp)
            || codec.Info.Width < 1 || codec.Info.Width > maxDimension
            || codec.Info.Height < 1 || codec.Info.Height > maxDimension)
        {
            return null;
        }

        using var bitmap = SKBitmap.Decode(codec);

        return bitmap is null ? null : SKImage.FromBitmap(bitmap);
    }

    private static void DrawLogo(SKCanvas canvas, ArtFormat format, LogoSlot slot, SKImage logo)
    {
        // O logo é encaixado em um quadrado, mantendo a proporção original.
        var side = (float)(slot.Size * format.Height);
        var scale = Math.Min(side / logo.Width, side / logo.Height);
        var width = logo.Width * scale;
        var height = logo.Height * scale;

        var centerX = (float)(slot.X * format.Width);
        var centerY = (float)(slot.Y * format.Height);

        var destination = SKRect.Create(centerX - (width / 2), centerY - (height / 2), width, height);

        // Filtro linear com mipmap: reduz o logo sem serrilhado.
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);

        canvas.DrawImage(logo, destination, sampling);
    }

    // ---------- Texto ----------

    private static void DrawText(SKCanvas canvas, ArtFormat format, TextLayer layer, string text)
    {
        var maxWidth = (float)(layer.MaxWidth * format.Width);

        // Só o título pode ocupar mais de uma linha.
        var maxLines = layer.Role == TextRole.LiveTitle ? 3 : 1;

        using var font = new SKFont(FontLibrary.Get(layer.Font), (float)(layer.FontSize * format.Height))
        {
            Subpixel = true,
            Edging = SKFontEdging.Antialias,
        };

        var lines = FitText(font, text, maxWidth, maxLines);

        using var paint = new SKPaint { Color = SKColor.Parse(layer.Color), IsAntialias = true };

        var align = layer.Align switch
        {
            TextAlign.Center => SKTextAlign.Center,
            TextAlign.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left,
        };

        var x = (float)(layer.X * format.Width);

        // O Skia posiciona o texto pela linha de base (onde as letras "sentam").
        // Como o layout informa o TOPO do texto, desce a altura de uma maiúscula.
        var capHeight = font.Metrics.CapHeight > 0 ? font.Metrics.CapHeight : -font.Metrics.Ascent;
        var baseline = (float)(layer.Y * format.Height) + capHeight;

        foreach (var line in lines)
        {
            canvas.DrawText(line, x, baseline, align, font, paint);
            baseline += font.Size * LineHeight;
        }
    }

    /// <summary>
    /// Faz o texto caber: quebra em linhas e, se não couber, encolhe a fonte aos poucos.
    /// Se nem no tamanho mínimo couber, corta com reticências.
    /// Ao terminar, <paramref name="font"/> fica com o tamanho escolhido.
    /// </summary>
    private static List<string> FitText(SKFont font, string text, float maxWidth, int maxLines)
    {
        var minSize = font.Size * MinFontScale;

        while (true)
        {
            var lines = Wrap(font, text, maxWidth);

            var fits = lines.Count <= maxLines && lines.All(line => font.MeasureText(line) <= maxWidth);

            if (fits)
            {
                return lines;
            }

            if (font.Size <= minSize)
            {
                return Truncate(font, lines, maxWidth, maxLines);
            }

            font.Size = Math.Max(minSize, font.Size * 0.94f);
        }
    }

    /// <summary>Quebra o texto em linhas, palavra por palavra, sem passar da largura.</summary>
    private static List<string> Wrap(SKFont font, string text, float maxWidth)
    {
        var lines = new List<string>();
        var current = string.Empty;

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = current.Length == 0 ? word : $"{current} {word}";

            if (current.Length > 0 && font.MeasureText(candidate) > maxWidth)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = candidate;
            }
        }

        if (current.Length > 0)
        {
            lines.Add(current);
        }

        return lines;
    }

    private static List<string> Truncate(SKFont font, List<string> lines, float maxWidth, int maxLines)
    {
        var kept = lines.Take(maxLines).ToList();

        // O que sobrou das linhas cortadas se junta à última, para o corte ser no fim do texto.
        if (lines.Count > maxLines)
        {
            kept[^1] = string.Join(' ', lines.Skip(maxLines - 1));
        }

        for (var i = 0; i < kept.Count; i++)
        {
            kept[i] = Ellipsize(font, kept[i], maxWidth);
        }

        return kept;
    }

    private static string Ellipsize(SKFont font, string line, float maxWidth)
    {
        if (font.MeasureText(line) <= maxWidth)
        {
            return line;
        }

        var text = line;

        while (text.Length > 1 && font.MeasureText(text + "…") > maxWidth)
        {
            text = text[..^1].TrimEnd();
        }

        return text + "…";
    }
}
