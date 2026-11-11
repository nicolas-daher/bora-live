using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BoraLive.Api.Tests.Support;
using BoraLive.Application.Art;
using BoraLive.Application.Formats;
using BoraLive.Application.Templates;

namespace BoraLive.Api.Tests;

// Estes testes desenham de verdade, com o SkiaSharp e as fontes embutidas.
// Rodam igual no Windows e no Linux do CI.
public sealed class ArtEndpointTests(BoraLiveApiFactory factory) : IClassFixture<BoraLiveApiFactory>
{
    // Um PNG válido de 1x1 pixel, para servir de logo.
    private static readonly byte[] TinyPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private readonly HttpClient _client = factory.CreateClient();

    private Task<HttpResponseMessage> PostArtAsync(
        string templateId = "cs2-tatico",
        string format = "thumbnail",
        string streamerName = "mare_alta",
        string liveTitle = "Subindo de elo, sem tilt",
        byte[]? logo = null)
    {
        // MultipartFormDataContent monta o mesmo corpo que um formulário com anexo do navegador.
        var form = new MultipartFormDataContent
        {
            { new StringContent(templateId), "templateId" },
            { new StringContent(format), "format" },
            { new StringContent(streamerName), "streamerName" },
            { new StringContent(liveTitle), "liveTitle" },
        };

        if (logo is not null)
        {
            var file = new ByteArrayContent(logo);
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(file, "logo", "logo.png");
        }

        return _client.PostAsync("/api/art", form);
    }

    // Todo PNG começa com a mesma assinatura e guarda largura e altura
    // nos bytes 16 a 23. Dá para conferir o tamanho sem biblioteca de imagem.
    private static (int Width, int Height) ReadPngSize(byte[] png)
    {
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.Equal(signature, png[..8]);

        return (
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4)));
    }

    private static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("code").GetString();
    }

    [Fact]
    public async Task PostArt_EveryTemplateInEveryFormat_ReturnsPngWithTheExactSize()
    {
        var templates = await _client.GetFromJsonAsync<TemplateDto[]>("/api/templates");
        var formats = await _client.GetFromJsonAsync<FormatDto[]>("/api/formats");

        foreach (var template in templates!)
        {
            foreach (var formatKey in template.Formats)
            {
                var expected = formats!.Single(format => format.Key == formatKey);

                var response = await PostArtAsync(template.Id, formatKey, logo: TinyPng);

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

                var size = ReadPngSize(await response.Content.ReadAsByteArrayAsync());
                Assert.Equal((expected.Width, expected.Height), size);
            }
        }
    }

    [Fact]
    public async Task PostArt_WithoutLogo_StillReturnsPng()
    {
        var response = await PostArtAsync(logo: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((1280, 720), ReadPngSize(await response.Content.ReadAsByteArrayAsync()));
    }

    [Fact]
    public async Task PostArt_SuggestsADownloadFileName()
    {
        var response = await PostArtAsync("valorant-ace", "story");

        Assert.Equal("bora-live-valorant-ace-story.png", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task PostArt_LongTitleWithAccentsAndEmoji_DoesNotBreak()
    {
        var response = await PostArtAsync(liveTitle: "Só vitória hoje: ação, emoção e coração 🔥 até de madrugada");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PostArt_UnknownTemplate_Returns404()
    {
        var response = await PostArtAsync(templateId: "nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("art.template_not_found", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_UnknownFormat_Returns400()
    {
        var response = await PostArtAsync(format: "poster");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art_format.unknown", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_TitleAboveTheLimit_Returns400WithDomainError()
    {
        var response = await PostArtAsync(liveTitle: new string('a', 61));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art_request.live_title_too_long", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_LogoThatIsNotAnImage_Returns400()
    {
        var response = await PostArtAsync(logo: "isto não é uma imagem"u8.ToArray());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("art.logo_invalid", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task PostArt_LogoAboveTheLimit_Returns413()
    {
        var response = await PostArtAsync(logo: new byte[ArtErrors.MaxLogoBytes + 1]);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal("art.logo_too_large", await ReadErrorCodeAsync(response));
    }
}
