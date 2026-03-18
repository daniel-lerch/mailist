using ConsoleAppFramework;
using Mailist.ChurchTools;
using Mailist.EmailDelivery;
using Mailist.EmailRelay;
using Mailist.Extensions;
using Mailist.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Mailist;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Create ConsoleAppFramework application and configure services from appsettings
        var cli = ConsoleApp.Create()
            .ConfigureDefaultConfiguration()
            .ConfigureLogging(builder =>
            {
                builder.AddConsole();
            })
            .ConfigureServices((context, configuration, services) =>
            {
                services.AddMailistOptions(configuration);
                services.AddMailistMySqlDatabase();
            });

        // Register default command to start backend
        cli.Add("", async (ConsoleAppContext context) =>
        {
            var builder = WebApplication.CreateBuilder(context.Arguments);

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            Configure(app, app.Environment);

            // Automatic database migration is done here right after building the host and not in Configure
            // to make sure no other hosted services can start while migrations are not yet completed.
            app.MigrateDatabase();

            await app.RunAsync();
        });

        // Register command class DatabaseCommand (methods become commands)
        cli.Add<DatabaseCommand>("database");

        await cli.RunAsync(args);
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddMailistOptions(configuration);

        services.AddChurchToolsApi();

        services.AddMailistMySqlDatabase();

        services.AddMemoryCache();

        services.AddControllers();

        services.AddOpenApiDocument();

        services.AddHealthChecks();

        services.AddOAuthAuthentication(configuration, environment);

        services.AddHostedService<ChurchToolsPermissionsHostedService>();

        services.AddSingleton<TokenService>();

        services.AddSingleton<ChurchQueryCacheService>();

        if (configuration.GetValue<bool>("EmailDelivery:Enable"))
        {
            services.AddSingleton<JobQueue<EmailDeliveryJobController>>();
            services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<JobQueue<EmailDeliveryJobController>>());
            services.AddScoped<EmailDeliveryService>();

            if (configuration.GetValue<bool>("EmailRelay:Enable"))
            {
                services.AddSingleton<JobQueue<EmailRelayJobController>>();
                services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<JobQueue<EmailRelayJobController>>());
                services.AddScoped<ImapReceiverService>();
                services.AddScoped<DistributionListService>();
                services.AddScoped<MimeMessageCreationService>();
                services.AddHostedService<EmailRelayHostedService>();
            }
        }
    }

    private static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHosting();

        app.UseRouting();

        app.UseMailistCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/healthz").RequireHost("localhost:*");
        });

        app.UseOpenApi();
        app.UseSwaggerUi();
    }
}
