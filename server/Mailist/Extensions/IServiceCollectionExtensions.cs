using Mailist.ChurchTools;
using Mailist.Configuration;
using Mailist.EmailDelivery;
using Mailist.EmailRelay;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace Mailist.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMailistOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection("Database"))
            .ValidateDataAnnotations();
        services.AddOptions<HostingOptions>()
            .Bind(configuration.GetSection("Hosting"))
            .ValidateDataAnnotations();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations();
        services.AddOptions<ChurchToolsOptions>()
            .Bind(configuration.GetSection("ChurchTools"))
            .ValidateDataAnnotations();
        services.AddOptions<EmailDeliveryOptions>()
            .Bind(configuration.GetSection("EmailDelivery"))
            .Validate(options =>
            {
                if (!options.Enable) return true;
                ValidationContext context = new(options);
                return Validator.TryValidateObject(options, context, null);
            });
        services.AddOptions<EmailRelayOptions>()
            .Bind(configuration.GetSection("EmailRelay"))
            .Validate(options =>
            {
                if (!options.Enable) return true;
                ValidationContext context = new(options);
                return Validator.TryValidateObject(options, context, null);
            });

        return services;
    }

    public static IServiceCollection AddMailistMySqlDatabase(this IServiceCollection services)
    {
        services.AddDbContextPool<DatabaseContext>((services, optionsBuilder) =>
        {
            var options = services.GetRequiredService<IOptions<DatabaseOptions>>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.UseMySql(
                options.Value.ConnectionString,
                ServerVersion.AutoDetect(options.Value.ConnectionString),
                builder => builder.EnableRetryOnFailure());
        });

        return services;
    }

    public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = null; // Disable automatic challenge of OAuth
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<ILogger<Program>, IOptions<JwtOptions>>((options, logger, jwtConfig) =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.Value.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Convert.FromHexString(jwtConfig.Value.SigningKey)),
                };
            });

        return services;
    }
}
