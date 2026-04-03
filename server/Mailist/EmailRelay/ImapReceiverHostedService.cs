using Mailist.EmailRelay.Entities;
using Mailist.Extensions;
using Mailist.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailRelay;

public class ImapReceiverHostedService : BackgroundService
{
    private readonly ILogger<ImapReceiverHostedService> logger;
    private readonly IOptions<EmailRelayOptions> options;
    private readonly JobQueue<EmailRelayJobController> jobQueue;
    private readonly IServiceProvider serviceProvider;

    public ImapReceiverHostedService(ILogger<ImapReceiverHostedService> logger, IOptions<EmailRelayOptions> options, JobQueue<EmailRelayJobController> jobQueue, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.options = options;
        this.jobQueue = jobQueue;
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using ImapClient imap = new();
        await Connect(imap, stoppingToken);

        MessageSummaryItems fetchItems =
            MessageSummaryItems.UniqueId | MessageSummaryItems.Flags | MessageSummaryItems.Headers | MessageSummaryItems.Body;

        while (true)
        {
            try
            {
                await foreach (IMessageSummary message in imap.IdleFetchAsync(imap.Inbox, fetchItems, stoppingToken))
                {
                    await ProcessMessage(message, stoppingToken);
                }
            }
            catch (ImapProtocolException)
            {
                await Connect(imap, stoppingToken);
            }
            catch (IOException)
            {
                await Connect(imap, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task Connect(ImapClient imap, CancellationToken cancellationToken)
    {
        if (!imap.IsConnected)
        {
            await imap.ConnectAsync(options.Value.ImapHost, options.Value.ImapPort, options.Value.ImapUseSsl, cancellationToken);
        }

        if (!imap.IsAuthenticated)
        {
            await imap.AuthenticateAsync(options.Value.ImapUsername, options.Value.ImapPassword, cancellationToken);
            await imap.Inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);
        }
    }

    private async Task ProcessMessage(IMessageSummary message, CancellationToken stoppingToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        InboxEmail? savedEmail = await database.InboxEmails.SingleOrDefaultAsync(email => email.UniqueId == message.UniqueId.Id, stoppingToken);

        if (savedEmail == null)
        {
            // Leave this message as is if it has been read by a user other than Mailist
            if (message.Flags!.Value.HasFlag(MessageFlags.Seen)) return;

            await QueueEmailForProcessing(message, database, stoppingToken);

            await message.Folder.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, stoppingToken);
        }
        else
        {
            // Check if message has been downloaded but not marked as seen
            if (!message.Flags!.Value.HasFlag(MessageFlags.Seen))
                await message.Folder.AddFlagsAsync(message.UniqueId, MessageFlags.Seen, silent: true, stoppingToken);

            // Delete email if download is longer ago than imap prune interval
            if (savedEmail.DownloadTime < DateTime.UtcNow.AddDays(-options.Value.ImapRetentionIntervalInDays))
            {
                logger.LogDebug("Pruning message {Id} from IMAP inbox", savedEmail.Id);
                await message.Folder.AddFlagsAsync(message.UniqueId, MessageFlags.Deleted, silent: true, stoppingToken);
            }
        }

        await message.Folder.ExpungeAsync(stoppingToken);
    }

    private async Task QueueEmailForProcessing(IMessageSummary message, DatabaseContext database, CancellationToken stoppingToken)
    {
        byte[]? headerContent = null;

        using (System.IO.MemoryStream memoryStream = new())
        {
            // Writing to a MemoryStream is a synchronous operation that won't be cancelled anyhow
            message.Headers.WriteTo(memoryStream, CancellationToken.None);
            if (memoryStream.Length <= options.Value.MaxHeaderSizeInKilobytes * 1024)
                headerContent = memoryStream.ToArray();
        }

        byte[]? bodyContent = null;

        // Dispose body and memoryStream directly after use to limit memory consumption
        using (MimeEntity body = await message.Folder.GetBodyPartAsync(message.UniqueId, message.Body, stoppingToken))
        using (System.IO.MemoryStream memoryStream = new())
        {
            // Writing to a MemoryStream is a synchronous operation that won't be cancelled anyhow
            body.WriteTo(memoryStream, CancellationToken.None);
            if (memoryStream.Length <= options.Value.MaxBodySizeInKilobytes * 1024)
                bodyContent = memoryStream.ToArray();
        }

        // According to RFC 5322 section 3.6, the Orig-Date and From headers are required
        // In case of missing headers, the message will be rejected in the next pipeline step
        string? from = message.Headers[HeaderId.From];
        string? to = message.Headers[HeaderId.To];
        string? receiver = message.Headers.GetReceiver();

        InboxEmail emailEntity = new(
            uniqueId: message.UniqueId.Id,
            subject: message.Headers[HeaderId.Subject],
            from: from,
            sender: message.Headers[HeaderId.Sender],
            replyTo: message.Headers[HeaderId.ReplyTo],
            to: to,
            receiver: receiver,
            header: headerContent,
            body: bodyContent);

        database.InboxEmails.Add(emailEntity);

        await database.SaveChangesAsync(stoppingToken);

        jobQueue.EnsureRunning();

        logger.LogInformation("Downloaded and stored message #{Id} from {From} for {Receiver}", emailEntity.Id, from, receiver ?? "an unknown receiver");
    }
}
