using System.ComponentModel.DataAnnotations;

namespace Mailist.SpamFilter;

public class SpamFilterOptions
{
    // Validation ensures that required values cannot be null
    public bool Enable { get; set; }
    [Required] public string ApiKey { get; set; } = null!;
    [Required] public string SystemPrompt { get; set; } = null!;
    public int MaxInputLength { get; set; }
}
