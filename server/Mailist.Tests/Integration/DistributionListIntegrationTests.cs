using Mailist.EmailRelay;
using Mailist.Models.Json;
using Mailist.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests.Integration;

public class DistributionListIntegrationTests : IClassFixture<MockChurchToolsApplicationFactory>
{
    private const string person17 = """
{"and":[{"==":[{"var":"person.isArchived"},0]},{"isnull":[{"var":"person.dateOfDeath"}]},{"==":[{"var":"person.id"},"17"]}]}
""";
    private const string group8 = """
{"and":[{"==":[{"var":"person.isArchived"},0]},{"isnull":[{"var":"person.dateOfDeath"}]},{"==":[{"var":"ctgroup.id"},"8"]},{"==":[{"var":"groupmember.groupMemberStatus"},"active"]}]}
""";

    private readonly MockChurchToolsApplicationFactory factory;

    public DistributionListIntegrationTests(MockChurchToolsApplicationFactory factory)
    {
        this.factory = factory;
    }

    private async Task<(long id, string alias)> InsertDistributionListAsync(string recipientsQuery, string sendersQuery)
    {
        string alias = $"unittest-{Guid.NewGuid()}";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var distributionList = new EmailRelay.Entities.DistributionList(alias, recipientsQuery, sendersQuery);
        db.DistributionLists.Add(distributionList);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (distributionList.Id, alias);
    }

    [Theory]
    [InlineData([group8, "null"])]
    [InlineData(["null", person17])]
    public async Task GetDistributionLists_ReturnsList(string recipientsQuery, string sendersQuery)
    {
        // Insert a distribution list into the application's database so the API returns it
        var (id, alias) = await InsertDistributionListAsync(recipientsQuery, sendersQuery);

        using HttpClient client = factory.CreateClient();

        // Create a token for subject "2" without admin permissions using the app's TokenService
        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("2", isAdmin: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var lists = await client.GetFromJsonAsync<DistributionList[]>("/api/distribution-lists", TestContext.Current.CancellationToken);
        Assert.NotNull(lists);
        var found = lists.FirstOrDefault(dl => dl.Alias == alias);
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(recipientsQuery, found.RecipientsQuery.GetRawText());
        Assert.Equal(0, found.RecipientCount);
        Assert.Equal(sendersQuery, found.SendersQuery.GetRawText());
        Assert.Equal(0, found.SenderCount);
    }

    [Theory]
    [InlineData([group8, "null"])]
    [InlineData(["null", person17])]
    public async Task GetDistributionListById_ReturnsItem(string recipientsQuery, string sendersQuery)
    {
        var (id, alias) = await InsertDistributionListAsync(recipientsQuery, sendersQuery);

        using HttpClient client = factory.CreateClient();

        // Create a token for subject "2" without admin permissions using the app's TokenService
        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("2", isAdmin: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var found = await client.GetFromJsonAsync<DistributionList>($"/api/distribution-lists/{id}", TestContext.Current.CancellationToken);
        Assert.NotNull(found);
        Assert.Equal(alias, found.Alias);
        Assert.Equal(id, found.Id);
        Assert.Equal(recipientsQuery, found.RecipientsQuery.GetRawText());
        Assert.Equal(0, found.RecipientCount);
        Assert.Equal(sendersQuery, found.SendersQuery.GetRawText());
        Assert.Equal(0, found.SenderCount);
    }

    [Theory]
    [InlineData([group8, "null"])]
    [InlineData(["null", person17])]
    public async Task CreateDistributionList_PersistsValues(string recipientsQuery, string sendersQuery)
    {
        using HttpClient client = factory.CreateClient();

        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("1", isAdmin: true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string alias = $"unittest-{Guid.NewGuid()}";

        var request = new CreateDistributionList
        {
            Alias = alias,
            Flags = new(DistributionListFlags.OverrideRecipient),
            RecipientsQuery = JsonElement.Parse(recipientsQuery),
            SendersQuery = JsonElement.Parse(sendersQuery),
        };

        var response = await client.PostAsJsonAsync("/api/distribution-lists", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<DistributionList>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal(alias, created!.Alias);
        Assert.True(created.Flags.OverrideRecipient);
        Assert.Equal(recipientsQuery, created.RecipientsQuery.GetRawText());
        Assert.Equal(sendersQuery, created.SendersQuery.GetRawText());
        Assert.Equal(0, created.RecipientCount);
        Assert.Equal(0, created.SenderCount);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var stored = await db.DistributionLists.SingleAsync(dl => dl.Id == created.Id, TestContext.Current.CancellationToken);
        Assert.Equal(alias, stored.Alias);
        Assert.Equal(DistributionListFlags.OverrideRecipient, stored.Flags);
        Assert.Equal(recipientsQuery, stored.RecipientsQuery);
        Assert.Equal(sendersQuery, stored.SendersQuery);
    }

    [Theory]
    [InlineData([group8, "null", "null", person17])]
    [InlineData(["null", group8, person17, "null"])]
    public async Task UpdateDistributionList_UpdatesRecipientsAndSendersQuery(
        string originalRecipientsQuery,
        string updatedRecipientsQuery,
        string originalSendersQuery,
        string updatedSendersQuery)
    {
        var (id, alias) = await InsertDistributionListAsync(originalRecipientsQuery, originalSendersQuery);

        using HttpClient client = factory.CreateClient();
        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("1", isAdmin: true);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var updateRequest = new CreateDistributionList
        {
            Alias = alias,
            Flags = new(DistributionListFlags.OverrideRecipient),
            RecipientsQuery = JsonElement.Parse(updatedRecipientsQuery),
            SendersQuery = JsonElement.Parse(updatedSendersQuery),
        };

        var response = await client.PutAsJsonAsync($"/api/distribution-lists/{id}", updateRequest, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<DistributionList>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updated);
        Assert.Equal(id, updated!.Id);
        Assert.Equal(alias, updated.Alias);
        Assert.True(updated.Flags.OverrideRecipient);
        Assert.Equal(updatedRecipientsQuery, updated.RecipientsQuery.GetRawText());
        Assert.Equal(updatedSendersQuery, updated.SendersQuery.GetRawText());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var stored = await db.DistributionLists.SingleAsync(dl => dl.Id == id, TestContext.Current.CancellationToken);
        Assert.Equal(alias, stored.Alias);
        Assert.Equal(DistributionListFlags.OverrideRecipient, stored.Flags);
        Assert.Equal(updatedRecipientsQuery, stored.RecipientsQuery);
        Assert.Equal(updatedSendersQuery, stored.SendersQuery);
    }
}
