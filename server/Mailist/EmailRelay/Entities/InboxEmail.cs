using Mailist.SpamFilter;
using System;

namespace Mailist.EmailRelay.Entities;

public class InboxEmail
{
    public InboxEmail(uint? uniqueId, string? subject, string? from, string? sender, string? replyTo, string? to, string? receiver, byte[]? header, byte[]? body)
    {
        UniqueId = uniqueId;
        Subject = subject;
        From = from;
        Sender = sender;
        ReplyTo = replyTo;
        To = to;
        Receiver = receiver;
        Header = header;
        Body = body;
    }

    public long Id { get; set; }

    public long? DistributionListId { get; set; }
    public DistributionList? DistributionList { get; set; }

    public uint? UniqueId { get; set; }
    public string? Subject { get; set; }
    public string? From { get; set; }
    public string? Sender { get; set; }
    public string? ReplyTo { get; set; }
    public string? To { get; set; }
    /// <summary>
    /// The mailbox this email was delivered to via SMTP.
    /// Determined by <c>Received</c>, <c>Envelope-To</c> or <c>X-Envelope-To</c> headers.
    /// If none of these headers was present, this value is <see langword="null"/>.
    /// </summary>
    public string? Receiver { get; set; }
    /// <summary>
    /// Original headers as received via IMAP, serialized by MimeKit.
    /// If the headers exceeded a certain size limit, this value is <see langword="null"/>.
    /// </summary>
    public byte[]? Header { get; set; }
    /// <summary>
    /// Original body as received via IMAP, serialized by MimeKit.
    /// If the body exceeded a certain size limit, this value is <see langword="null"/>.
    /// </summary>
    public byte[]? Body { get; set; }
    public SpamCategory SpamCategory { get; set; }
    public string? SpamJustification { get; set; }
    public DateTime DownloadTime { get; set; }
    public DateTime ProcessingCompletedTime { get; set; }
}
