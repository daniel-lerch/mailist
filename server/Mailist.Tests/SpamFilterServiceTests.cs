using Mailist.EmailRelay.Entities;
using Mailist.SpamFilter;
using Microsoft.Extensions.AI;
using Mistral.SDK;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class SpamFilterServiceTests
{
    private readonly SpamFilterService spamFilterService;

    public SpamFilterServiceTests()
    {
        // TODO: Read API Key from configuration
        IChatClient client = new MistralClient().Completions;
        spamFilterService = new(client, new());
    }

    [Fact]
    public async Task Test()
    {
        InboxEmail email = new(
            uniqueId: null,
            subject: "Massagesessel als luxuriöser Leder-Chefsessel mit Liegefunktion – für Ihre Gesundheit",
            from: "Premium Box <susanne.kern@news.premium-box.eu>",
            sender: null,
            replyTo: "info@premiumboxx.de",
            to: "kontakt@christuskirche.com",
            receiver: "kontakt@christuskirche.com",
            header: Encoding.UTF8.GetBytes(LoadManifestResourceText("23437.header")),
            body: Encoding.UTF8.GetBytes(LoadManifestResourceText("23437.body")));

        await spamFilterService.ClassifyMessage(email, TestContext.Current.CancellationToken);
    }

    private static string LoadManifestResourceText(string resourceFileName)
    {
        string resourceName = $"Mailist.Tests.Resources.{resourceFileName}";

        using Stream? stream = typeof(SpamFilterServiceTests).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Manifest resource stream '{resourceName}' was not found.");

        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
