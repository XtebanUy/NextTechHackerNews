
using Flurl.Http.Configuration;
using Microsoft.Extensions.Configuration;
using NextTech.Application.Interfaces;
using NextTech.Infrastructure.Configuration;
using NextTech.Infrastructure.Services;


namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfraestructureSevices(
            this IServiceCollection services, IConfiguration config)
    {
        services.Configure<HackerNewsApiOptions>(config.GetSection(HackerNewsApiOptions.HackerNewsApi));
        services.AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>();
        services.AddSingleton<IRepositoryService, RepositoryService>();

        return services;
    }
}

