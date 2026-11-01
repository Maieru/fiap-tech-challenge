using FIAP.TechChallenge.OS.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.OS.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        _ = services.AddSingleton(TimeProvider.System);
        _ = services.AddScoped<MessageContext>();

        foreach (var type in typeof(ApplicationDependencyInjection).Assembly.GetTypes()
                     .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("UseCase", StringComparison.Ordinal)))
        {
            foreach (var contract in type.GetInterfaces())
                _ = services.AddScoped(contract, type);
        }
        return services;
    }
}
