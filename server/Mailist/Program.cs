using Mailist.Commands;
using Mailist.ChurchTools;
using Mailist.EmailDelivery;
using Mailist.EmailRelay;
using Mailist.Extensions;
using Mailist.Utilities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Mailist;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // TODO: Make the base command execute the web host (WebApplicationFactory passes ~3 args)
        //if (args.Length == 0)
        //{
        var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();

            Configure(app, app.Environment);

            // Automatic database migration is done here right after building the host and not in Configure
            // to make sure no other hosted services can start while migrations are not yet completed.
            app.MigrateDatabase();

            await app.RunAsync();
            return Environment.ExitCode;
        //}
        //else
        //{
        //    return await CreateAndRunCommandLine(args);
        //}
    }

    private static async Task<int> CreateAndRunCommandLine(string[] args)
    {
        IServiceScope? scope = null;
        try
        {
            return await CreateCliHostBuilder().RunCommandLineApplicationAsync<MailistCommand>(args, app =>
            {
                var scope = app.CreateScope();
                app.Conventions.UseConstructorInjection(scope.ServiceProvider);
            });
        }
        catch (UnrecognizedCommandParsingException)
        {
            return 1;
        }
        finally
        {
            scope?.Dispose();
        }
    }

    public static IHostBuilder CreateCliHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMailistOptions(context.Configuration);
                services.AddSingleton(PhysicalConsole.Singleton);
                services.AddMailistMySqlDatabase();
            });

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddMailistOptions(configuration);

        services.AddChurchToolsApi();

        services.AddMailistMySqlDatabase();

        services.AddMemoryCache();

        services.AddControllers();

        services.AddOpenApiDocument();

        services.AddHealthChecks();

        if (!environment.IsDevelopment())
        {
            services.AddDataProtection().PersistKeysToFileSystem(new(System.IO.Path.Combine(environment.ContentRootPath, "secrets")));
        }

        services.AddOAuthAuthentication(configuration, environment);

        services.AddHostedService<ChurchToolsPermissionsHostedService>();

        services.AddSingleton<TokenService>();

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
