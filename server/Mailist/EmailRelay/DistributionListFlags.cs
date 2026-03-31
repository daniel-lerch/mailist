using System;

namespace Mailist.EmailRelay;

[Flags]
public enum DistributionListFlags
{
    None = 0,
    OverrideRecipient = 1,
    SpamFilter = 2,
}
