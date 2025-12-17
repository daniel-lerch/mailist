using Mailist.EmailDelivery;
using Mailist.EmailRelay.Entities;
using Mailist.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailRelay;

public class EmailRelayJobController : OneAtATimeJobController<InboxEmail>
{
    private readonly DatabaseContext database;
    private readonly ILogger<EmailRelayJobController> logger;
    private readonly DistributionListService distributionListService;
    private readonly MimeMessageCreationService emailRelay;
    private readonly EmailDeliveryService emailDelivery;

    public EmailRelayJobController(DatabaseContext database, ILogger<EmailRelayJobController> logger, DistributionListService distributionListService, MimeMessageCreationService emailRelay, EmailDeliveryService emailDelivery)
    {
        this.database = database;
        this.logger = logger;
        this.distributionListService = distributionListService;
        this.emailRelay = emailRelay;
        this.emailDelivery = emailDelivery;
    }

    protected override async ValueTask<InboxEmail?> NextPendingOrDefault(CancellationToken cancellationToken)
    {
        return await database.InboxEmails
            .Where(email => email.ProcessingCompletedTime == default)
            .OrderBy(email => email.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected override async ValueTask ExecuteJob(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.From == null)
        {
            await RejectEmail(email, errorMessage: null, cancellationToken);

            logger.LogWarning("Email #{Id} has no From header and will be rejected. " +
                "According to RFC 5311 section 3.6, emails must include a From header. " +
                "Please contact your email service provider.", email.Id);
            return;
        }

        if (email.Receiver == null)
        {
            using MimeMessage? errorMessage = emailRelay.InvalidServerConfiguration(email);
            await RejectEmail(email, errorMessage, cancellationToken);

            logger.LogWarning("Could not determine receiver for message #{Id} from {From} to {To}. This message will not be forwarded. " +
                "Please make sure your email provider specifies the receiver in the Received, Envelope-To, or X-Envelope-To header", email.Id, email.From, email.To);
            return;
        }

        int atIdx = email.Receiver.IndexOf('@');
        string emailAlias = email.Receiver.Remove(atIdx);

        DistributionList? distributionList = await database.DistributionLists.SingleOrDefaultAsync(x => x.Alias == emailAlias, cancellationToken);

        if (distributionList == null)
        {
            using MimeMessage? errorMessage = emailRelay.InvalidAlias(email);
            await RejectEmail(email, errorMessage, cancellationToken);

            logger.LogInformation("No group found with alias {Receiver} for email #{Id} from {From}", email.Receiver, email.Id, email.From);
            return;
        }

        // TODO: Check allowed senders
        if (await distributionListService.IsSenderPermitted(distributionList))
        {

        }

        if (email.Header == null)
        {
            using MimeMessage? errorMessage = emailRelay.TooManyHeaders(email);
            await RejectEmail(email, errorMessage, cancellationToken);

            logger.LogInformation("Email #{Id} from {From} to {Receiver} exceeded the header size limit", email.Id, email.From, email.Receiver);
            return;
        }

        if (email.Body == null)
        {
            using MimeMessage? errorMessage = emailRelay.TooBigMessage(email);
            await RejectEmail(email, errorMessage, cancellationToken);

            logger.LogInformation("Email #{Id} from {From} to {Receiver} exceeded the body size limit", email.Id, email.From, email.Receiver);
            return;
        }

        MailboxAddress[] recipients = await distributionListService.GetRecipients(distributionList, cancellationToken);
        foreach (MailboxAddress address in recipients)
        {
            using MimeMessage preparedMessage = distributionList.Flags.HasFlag(DistributionListFlags.Newsletter)
                ? await emailRelay.PrepareForForwardTo(email, address, cancellationToken)
                : emailRelay.PrepareForResentTo(email, address);
            await emailDelivery.Enqueue(address.Address, preparedMessage, email.Id, cancellationToken);
        }
        email.DistributionListId = distributionList.Id;
        email.ProcessingCompletedTime = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fetched {RecipientsCount} recipients for email #{Id} to {Receiver}", recipients.Length, email.Id, email.Receiver);
        return;
    }

    private async ValueTask RejectEmail(InboxEmail email, MimeMessage? errorMessage, CancellationToken cancellationToken)
    {
        if (errorMessage != null)
            await emailDelivery.Enqueue(((MailboxAddress)errorMessage.To[0]).Address, errorMessage, email.Id, cancellationToken);

        email.ProcessingCompletedTime = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
    }

    private static string? GetActualSender(InboxEmail email)
    {
        if (email.Sender != null)
        {
            if (MailboxAddress.TryParse(email.Sender, out MailboxAddress mailboxAddress))
                return mailboxAddress.Address;
            else
                return null;
        }
        else
        {
            return MailboxAddressHelper.FirstMailboxAddressOrDefault(email.From)?.Address;
        }
    }
}
