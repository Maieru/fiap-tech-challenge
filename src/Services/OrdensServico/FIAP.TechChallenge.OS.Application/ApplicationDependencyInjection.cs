using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.OS.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        foreach (var type in typeof(ApplicationDependencyInjection).Assembly.GetTypes()
                     .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("UseCase", StringComparison.Ordinal)))
        {
            foreach (var contract in type.GetInterfaces()) services.AddScoped(contract, type);
        }
        return services;
    }
}
