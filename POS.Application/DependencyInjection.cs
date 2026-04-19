using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace POS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register AutoMapper
        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}
