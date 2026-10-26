using System;
using System.ComponentModel.DataAnnotations;

namespace Mailist.SpamFilter;

public class SpamFilterOptions
{
    // Validation ensures that required values cannot be null
    public bool Enable { get; set; }
    [Required] public string ApiKey { get; set; } = null!;
    [Required] public string SystemPrompt { get; set; } = null!;
    public int MaxInputLength { get; set; }
    // Maximum total time spent retrying transient chat client failures before classification fails
    public TimeSpan MaxRetryDuration { get; set; } = TimeSpan.FromMinutes(2);
}
