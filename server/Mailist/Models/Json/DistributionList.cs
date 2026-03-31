using Mailist.EmailRelay;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mailist.Models.Json;

public class CreateDistributionList
{
    public required string Alias { get; init; }
    public required DistributionListFlagsObject Flags { get; init; }
    public required JsonElement RecipientsQuery { get; init; }
    public required JsonElement SendersQuery { get; init; }
}

public class DistributionList : CreateDistributionList
{
    public required long Id { get; init; }
    public int RecipientCount { get; set; }
    public int SenderCount { get; set; }
}

public class DistributionListFlagsObject
{
    [JsonConstructor]
    public DistributionListFlagsObject(bool overrideRecipient, bool spamFilter)
    {
        OverrideRecipient = overrideRecipient;
        SpamFilter = spamFilter;
    }

    public DistributionListFlagsObject(DistributionListFlags flags)
    {
        OverrideRecipient = flags.HasFlag(DistributionListFlags.OverrideRecipient);
        SpamFilter = flags.HasFlag(DistributionListFlags.SpamFilter);
    }

    public bool OverrideRecipient { get; }
    public bool SpamFilter { get; }

    public static implicit operator DistributionListFlags(DistributionListFlagsObject obj)
    {
        DistributionListFlags flags = DistributionListFlags.None;
        if (obj.OverrideRecipient)
            flags |= DistributionListFlags.OverrideRecipient;
        if (obj.SpamFilter)
            flags |= DistributionListFlags.SpamFilter;
        return flags;
    }
}
