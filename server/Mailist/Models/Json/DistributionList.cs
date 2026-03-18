using System.Text.Json;

namespace Mailist.Models.Json;

public class CreateDistributionList
{
    public required string Alias { get; init; }
    public required bool Newsletter { get; init; }
    public required JsonElement RecipientsQuery { get; init; }
    public required JsonElement SendersQuery { get; init; }
}

public class DistributionList : CreateDistributionList
{
    public required long Id { get; init; }
    public int RecipientCount { get; set; }
    public int SenderCount { get; set; }
}
