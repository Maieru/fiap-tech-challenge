using FIAP.TechChallenge.Fase1.Domain.Interfaces;
using FIAP.TechChallenge.Fase1.Infrastructure.Notification.Mail;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence;
using FIAP.TechChallenge.Fase1.Infrastructure.Persistence.Repositories;
using FIAP.TechChallenge.Fase1.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FIAP.TechChallenge.Fase1.Infrastructure;

public static class InfraestructureDependecyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ConfigureAuthentication(services, configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
            _ = services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        _ = services.AddScoped<IClienteRepository, ClienteRepository>();
        _ = services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        _ = services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        _ = services.AddScoped<IPecaInsumoRepository, PecaInsumoRepository>();
        _ = services.AddScoped<IPecaOuInsumoDaOrdemDeServicoRepository, PecaOuInsumoDaOrdemDeServicoRepository>();
        _ = services.AddScoped<IServicoRepository, ServicoRepository>();
        _ = services.AddScoped<IServicoDaOrdemDeServicoRepository, ServicoDaOrdemDeServicoRepository>();
        _ = services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        _ = services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

        _ = services.AddScoped<IMailService, MailService>();

        return services;
    }

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var signingKey = jwtSection["SigningKey"];
        var accessTokenMinutesRaw = jwtSection["AccessTokenMinutes"];

        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Jwt:Issuer must be configured.");

        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Jwt:Audience must be configured.");

        if (string.IsNullOrWhiteSpace(signingKey))
            throw new InvalidOperationException("Jwt:SigningKey must be configured.");

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey must have at least 32 bytes.");

        if (!int.TryParse(accessTokenMinutesRaw, out var accessTokenMinutes) || accessTokenMinutes <= 0)
            throw new InvalidOperationException("Jwt:AccessTokenMinutes must be a positive integer.");

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        _ = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricSecurityKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        _ = services.AddAuthorization();
        _ = services.AddSingleton<ITokenService>(_ => new JwtTokenService(issuer, audience, symmetricSecurityKey, accessTokenMinutes));
    }
}
