using System.ComponentModel.DataAnnotations;

namespace Mailist.EmailDelivery;

public class EmailDeliveryOptions
{
    // Validation ensures that required values cannot be null
    public bool Enable { get; set; }

    [Required] public string SenderName { get; set; } = null!;
    [Required] public string SenderAddress { get; set; } = null!;
    public string? ReturnPath { get; set; }
    [Required] public string SmtpHost { get; set; } = null!;
    [Range(1, 65535)] public int SmtpPort { get; set; }
    public bool SmtpUseSsl { get; set; }
    [Required] public string SmtpUsername { get; set; } = null!;
    [Required] public string SmtpPassword { get; set; } = null!;
}
