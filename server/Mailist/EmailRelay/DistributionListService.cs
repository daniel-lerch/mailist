using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay.Entities;
using MimeKit;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailRelay;

public class DistributionListService
{
    private readonly IChurchToolsApi churchTools;

    public DistributionListService(IChurchToolsApi churchTools)
    {
        this.churchTools = churchTools;
    }

    public async ValueTask<MailboxAddress[]> GetRecipients(DistributionList distributionList, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(distributionList.RecipientsQuery)) return [];

        JsonElement filter = JsonElement.Parse(distributionList.RecipientsQuery);
        ChurchQueryRequest<IdNameEmail> query = new(filter);

        var people = await churchTools.ChurchQuery(query, cancellationToken);

        return people
            .Where(p => !string.IsNullOrWhiteSpace(p.Email))
        // Avoid duplicate emails for married couples with a shared email address
            .GroupBy(p => p.Email)
            .Select(grouping => new MailboxAddress(
                name: string.Join(", ", grouping.Select(r => r.FirstName)) + ' ' + grouping.First().LastName,
                address: grouping.Key))
            .ToArray();
    }

    public async ValueTask<bool> IsSenderPermitted(string? senderEmail, DistributionList distributionList, CancellationToken cancellationToken)
    {
        JsonElement filter = JsonElement.Parse(distributionList.SendersQuery);
        if (filter.ValueKind == JsonValueKind.Null)
            return true;

        if (string.IsNullOrWhiteSpace(senderEmail))
            return false;

        ChurchQueryRequest<IdNameEmail> query = new(filter);
        var people = await churchTools.ChurchQuery(query, cancellationToken);
        return people.Any(p => p.Email.Equals(senderEmail, StringComparison.OrdinalIgnoreCase));
    }
}
