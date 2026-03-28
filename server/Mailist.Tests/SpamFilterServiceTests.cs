using Mailist.EmailRelay.Entities;
using Mailist.SpamFilter;
using Microsoft.Extensions.Configuration;
using Mistral.SDK;
using System.IO;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class SpamFilterServiceTests : IDisposable
{
    private readonly MistralClient? mistralClient;
    private readonly SpamFilterService? spamFilterService;

    public SpamFilterServiceTests()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        if (configuration.GetValue<bool>("SpamFilter:Enable"))
        {
            string apiKey = configuration["SpamFilter:ApiKey"]
                ?? throw new InvalidOperationException("Missing configuration value 'SpamFilter:ApiKey'.");

            mistralClient = new MistralClient(new APIAuthentication(apiKey));
            spamFilterService = new(mistralClient.Completions, new());
        }
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
        mistralClient?.Dispose();
    }

    private static byte[] LoadManifestResourceBytes(string resourceFileName)
    {
        string resourceName = $"Mailist.Tests.Resources.{resourceFileName}";

        using Stream? stream = typeof(SpamFilterServiceTests).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Manifest resource stream '{resourceName}' was not found.");

        byte[] buffer = new byte[stream.Length];

        stream.ReadExactly(buffer);

        return buffer;
    }
}
