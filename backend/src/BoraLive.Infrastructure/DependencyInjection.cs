using BoraLive.Application.Abstractions;
using BoraLive.Infrastructure.Gemini;
using BoraLive.Infrastructure.Templates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BoraLive.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra tudo o que a Infrastructure implementa.
    /// A Api chama só este método e não precisa conhecer as classes concretas.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));

        // "Quando alguém pedir IAiImageGenerator, entregue um GeminiImageGenerator
        // com um HttpClient configurado assim."
        services.AddHttpClient<IAiImageGenerator, GeminiImageGenerator>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        // Singleton: os modelos são lidos uma vez e reaproveitados por todas as requisições.
        services.AddSingleton<ITemplateRepository, JsonTemplateRepository>();

        return services;
    }
}
