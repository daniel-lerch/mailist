using Mailist.EmailDelivery.Entities;
using Mailist.Utilities;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailDelivery;

public class EmailDeliveryService
{
    private readonly DatabaseContext database;
    private readonly JobQueue<EmailDeliveryJobController> jobQueue;

    public EmailDeliveryService(DatabaseContext database, JobQueue<EmailDeliveryJobController> jobQueue)
    {
        this.database = database;
        this.jobQueue = jobQueue;
    }

    /// <summary>
    /// Enqueues a forwarded distribution list email without storing its content.
    /// The message is reconstructed from the referenced inbox email at delivery time, which avoids
    /// storing an identical MIME blob per recipient and keeps the OutboxEmails table free of large,
    /// high-churn BLOBs.
    /// </summary>
    public async ValueTask<bool> EnqueueForward(string emailAddress, long inboxEmailId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("The recipient address must not be null or empty", nameof(emailAddress));

        if (await database.OutboxEmails
                .AnyAsync(email => email.EmailAddress == emailAddress && email.InboxEmailId == inboxEmailId, cancellationToken))
            return false;

        OutboxEmail outboxEmail = new(emailAddress, []) { InboxEmailId = inboxEmailId };
        database.OutboxEmails.Add(outboxEmail);
        await database.SaveChangesAsync(cancellationToken);

        database.Entry(outboxEmail).State = EntityState.Detached;

        jobQueue.EnsureRunning();
        return true;
    }

    /// <summary>
    /// Enqueues a system message (e.g. a delivery error) by storing its complete serialized content.
    /// </summary>
    public async ValueTask<bool> Enqueue(string emailAddress, MimeMessage mimeMessage, long? inboxEmailId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("The recipient address must not be null or empty", nameof(emailAddress));

        if (inboxEmailId.HasValue && await database.OutboxEmails
                .AnyAsync(email => email.EmailAddress == emailAddress && email.InboxEmailId == inboxEmailId, cancellationToken))
            return false;

        byte[] content;

        using (MemoryStream memoryStream = new())
        {
            mimeMessage.WriteTo(memoryStream, CancellationToken.None);
            content = memoryStream.ToArray();
        }

        OutboxEmail outboxEmail = new(emailAddress, content) { InboxEmailId = inboxEmailId };
        database.OutboxEmails.Add(outboxEmail);
        await database.SaveChangesAsync(cancellationToken);

        // Explicitly free entity for garbage collection because our DbContext won't be disposed soon enough
        // Without this line, Mailist takes gigabytes of memory when sending large messages to many recipients
        database.Entry(outboxEmail).State = EntityState.Detached;

        jobQueue.EnsureRunning();
        return true;
    }
}
