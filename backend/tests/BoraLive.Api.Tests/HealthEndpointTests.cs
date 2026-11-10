using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BoraLive.Api.Tests;

// WebApplicationFactory sobe a API inteira em memória, sem abrir porta de rede.
// IClassFixture faz o xUnit criar a fábrica uma vez e reaproveitar nos testes da classe.
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Get_Health_ReturnsOkAndHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
