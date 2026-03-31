using Mailist.EmailRelay.Entities;
using Mailist.Extensions;
using Mailist.SpamFilter;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mistral.SDK;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests.Integration;

public class SpamFilterServiceIntegrationTests : IDisposable
{
    private readonly ServiceProvider? serviceProvider;
    private readonly SpamFilterService? spamFilterService;

    public SpamFilterServiceIntegrationTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        ServiceCollection services = new();
        services.AddMailistOptions(configuration);
        services.AddLogging();
        services.AddSingleton<MimeTextExtractionService>();
        services.AddSingleton<IChatClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SpamFilterOptions>>();
            return new MistralClient(new APIAuthentication(options.Value.ApiKey)).Completions;
        });
        services.AddSingleton<SpamFilterService>();
        serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<SpamFilterOptions>>();
        if (options.Value.Enable)
        {
            spamFilterService = serviceProvider.GetRequiredService<SpamFilterService>();
        }
    }

    [Fact]
    public async Task EasterPathIsClassifiedAsLegitimate()
    {
        Assert.SkipWhen(spamFilterService == null, "Spam filter service is not enabled in configuration.");
        InboxEmail email = new(
            uniqueId: null,
            subject: "Herzliche Einladung zu einem Osterweg in der FeG Jugenheim",
            from: "Christine <c***@fegsj.de>",
            sender: null,
            replyTo: null,
            to: "Christine <c***@t-online.de>",
            receiver: "kontakt@christuskirche.com",
            header: LoadManifestResourceBytes("23377.header"),
            body: LoadManifestResourceBytes("23377.body"));

        var result = await spamFilterService.ClassifyMessage(email, TestContext.Current.CancellationToken);
        Assert.Equal(SpamCategory.Legitimate, result.Category);
        Assert.NotEmpty(result.Justification);
    }

    [Fact]
    public async Task FurnitureIsClassifiedAsIrrelevant()
    {
        Assert.SkipWhen(spamFilterService == null, "Spam filter service is not enabled in configuration.");

        InboxEmail email = new(
            uniqueId: null,
            subject: "Massagesessel als luxuriöser Leder-Chefsessel mit Liegefunktion – für Ihre Gesundheit",
            from: "Premium Box <susanne.kern@news.premium-box.eu>",
            sender: null,
            replyTo: "info@premiumboxx.de",
            to: "kontakt@christuskirche.com",
            receiver: "kontakt@christuskirche.com",
            header: LoadManifestResourceBytes("23437.header"),
            body: LoadManifestResourceBytes("23437.body"));

        var result = await spamFilterService.ClassifyMessage(email, TestContext.Current.CancellationToken);
        Assert.Equal(SpamCategory.Irrelevant, result.Category);
        Assert.NotEmpty(result.Justification);
    }

    public void Dispose()
    {
        serviceProvider?.Dispose();
    }

    private static byte[] LoadManifestResourceBytes(string resourceFileName)
    {
        string resourceName = $"Mailist.Tests.Resources.{resourceFileName}";

        using Stream? stream = typeof(SpamFilterServiceIntegrationTests).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Manifest resource stream '{resourceName}' was not found.");

        byte[] buffer = new byte[stream.Length];

        stream.ReadExactly(buffer);

        return buffer;
    }
}
