using System.ComponentModel.DataAnnotations;

namespace Mailist.EmailRelay;

public class EmailRelayOptions
{
    // Validation ensures that required values cannot be null
    public bool Enable { get; set; }

    [Required] public string ImapHost { get; set; } = null!;
    [Range(1, 65535)] public int ImapPort { get; set; }
    public bool ImapUseSsl { get; set; }
    [Required] public string ImapUsername { get; set; } = null!;
    [Required] public string ImapPassword { get; set; } = null!;

    [Range(0.0, 87600.0 /* max. 10 years */)] public double ImapRetentionIntervalInDays { get; set; }
    [Range(16, 1024)] public int MaxHeaderSizeInKilobytes { get; set; }
    [Range(64, 131072)] public int MaxBodySizeInKilobytes { get; set; }
}
