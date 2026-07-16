using Mailist.EmailRelay.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mailist.EmailDelivery.Entities;

public class OutboxEmail
{
    public OutboxEmail(string emailAddress, byte[] content)
    {
        EmailAddress = emailAddress;
        Content = content;
    }

    public long Id { get; set; }

    public long? InboxEmailId { get; set; }
    public InboxEmail? InboxEmail { get; set; }

    public string EmailAddress { get; set; }

    /// <summary>
    /// The complete MIME message to send, serialized by MimeKit.
    /// For forwarded distribution list emails this is empty and the message is reconstructed
    /// on demand from the referenced <see cref="InboxEmail"/> at delivery time (see <see cref="IsForward"/>).
    /// Only system messages (e.g. delivery errors) carry their content here.
    /// </summary>
    public byte[] Content { get; set; }

    /// <summary>
    /// <see langword="true"/> if this email must be reconstructed from the referenced <see cref="InboxEmail"/>
    /// instead of sending the stored <see cref="Content"/>. Empty content is the discriminator because a
    /// serialized system message is never empty.
    /// </summary>
    [NotMapped]
    public bool IsForward => Content.Length == 0;
}
