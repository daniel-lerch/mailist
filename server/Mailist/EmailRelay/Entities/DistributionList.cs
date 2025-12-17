namespace Mailist.EmailRelay.Entities;

public class DistributionList
{
	public DistributionList(string alias, string sendersQuery, string recipientsQuery)
	{
		Alias = alias;
        SendersQuery = sendersQuery;
        RecipientsQuery = recipientsQuery;
    }

	public long Id { get; set; }
	public string Alias { get; set; }
	public DistributionListFlags Flags { get; set; }

    // If SendersQuery is "null", everybody is allowed to send emails to this distribution list.
    public string SendersQuery { get; set; }

    // If RecipientsQuery is "null", nobody will receive emails from this distribution list.
    public string RecipientsQuery { get; set; }
}
