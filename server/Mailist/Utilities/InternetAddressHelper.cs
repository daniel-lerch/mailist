using MimeKit;

namespace Mailist.Utilities;

public class InternetAddressHelper
{
    public static MailboxAddress? FirstMailboxAddressOrDefault(string? addressList)
    {
        if (addressList == null || !InternetAddressList.TryParse(addressList, out InternetAddressList? internetAddressList))
            return null;

        foreach (InternetAddress address in internetAddressList)
        {
            // We might find no MailboxAddress in a From header if all addresses are GroupAddresses
            if (address is MailboxAddress mailboxAddress)
                return mailboxAddress;
        }

        return null;
    }
}
