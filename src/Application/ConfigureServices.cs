using System.Reflection;
using FluentValidation;
using NextTech.Application.Common.Interfaces;
using NextTech.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSingleton<IStoryService, StoryService>();
        return services;
    }
}