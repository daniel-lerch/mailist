using Mailist.Models.Json;
using Mailist.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests.Integration;

public class DistributionListIntegrationTests : IClassFixture<MockChurchToolsApplicationFactory>
{
    private readonly MockChurchToolsApplicationFactory factory;

    public DistributionListIntegrationTests(MockChurchToolsApplicationFactory factory)
    {
        this.factory = factory;
    }

    private async Task<(long id, string alias)> InsertDistributionListAsync()
    {
        string alias = $"unittest-{Guid.NewGuid()}";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        var distributionList = new EmailRelay.Entities.DistributionList(alias, "null");
        db.DistributionLists.Add(distributionList);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (distributionList.Id, alias);
    }

    [Fact]
    public async Task GetDistributionLists_ReturnsList()
    {
        // Insert a distribution list into the application's database so the API returns it
        var (id, alias) = await InsertDistributionListAsync();

        using HttpClient client = factory.CreateClient();

        // Create a token for subject "2" without admin permissions using the app's TokenService
        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("2", isManager: false, isAdmin: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var lists = await client.GetFromJsonAsync<DistributionList[]>("/api/distribution-lists", TestContext.Current.CancellationToken);
        Assert.NotNull(lists);
        var found = lists.FirstOrDefault(dl => dl.Alias == alias);
        Assert.NotNull(found);
        Assert.Equal(id, found.Id);
        Assert.Equal(JsonValueKind.Null, found!.RecipientsQuery.ValueKind);
        Assert.Equal(0, found.RecipientCount);
    }

    [Fact]
    public async Task GetDistributionListById_ReturnsList()
    {
        var (id, alias) = await InsertDistributionListAsync();

        using HttpClient client = factory.CreateClient();

        // Create a token for subject "2" without admin permissions using the app's TokenService
        var tokenService = factory.Services.GetRequiredService<TokenService>();
        string token = tokenService.CreateToken("2", isManager: false, isAdmin: false);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var list = await client.GetFromJsonAsync<DistributionList>($"/api/distribution-lists/{id}", TestContext.Current.CancellationToken);
        Assert.NotNull(list);
        Assert.Equal(alias, list.Alias);
        Assert.Equal(id, list.Id);
        Assert.Equal(JsonValueKind.Null, list!.RecipientsQuery.ValueKind);
        Assert.Equal(0, list.RecipientCount);
    }
}
