using ChurchTools;
using Mailist.EmailRelay;
using Mailist.EmailRelay.Entities;
using Mailist.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class MimeMessageCreationServiceTests
{
    [Fact]
    public async Task TestNameLookup()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            // Integration tests work without user secrets like in a CI pipeline
            .Build();

        ServiceCollection services = new();
        services.AddMailistOptions(configuration);
        services.AddSingleton<IChurchToolsApi>(await ChurchToolsApi.Login(ChurchToolsApiTests.ChurchToolsHost, ChurchToolsApiTests.ChurchToolsUsername, ChurchToolsApiTests.ChurchToolsPassword));

        var serviceProvider = services.BuildServiceProvider();

        var emailRelay = ActivatorUtilities.CreateInstance<MimeMessageCreationService>(serviceProvider);

        byte[] header;
        using (MemoryStream headerStream = new())
        {
            var headerList = new HeaderList();
            await headerList.WriteToAsync(headerStream, TestContext.Current.CancellationToken);
            header = headerStream.ToArray();
        }

        byte[] body;
        using (MemoryStream bodyStream = new())
        {
            var bodyBuilder = new BodyBuilder
            {
                TextBody = "This is a test email."
            };
            await bodyBuilder.ToMessageBody().WriteToAsync(bodyStream, TestContext.Current.CancellationToken);
            body = bodyStream.ToArray();
        }

        InboxEmail inboxEmail = new(
            uniqueId: 1,
            subject: "Test",
            from: "support@example.com", // Armin Adendorf in demo.church.tools
            sender: null,
            replyTo: null,
            to: "test@example.org",
            receiver: "test@example.org",
            header: header,
            body: body);

        var mimeMessage = await emailRelay.PrepareForward(inboxEmail, TestContext.Current.CancellationToken);
        if (mimeMessage.From[0] is MailboxAddress from)
        {
            Assert.Equal("Armin Adendorf", from.Name);
        }
        else
        {
            Assert.Fail();
        }
    }
}
