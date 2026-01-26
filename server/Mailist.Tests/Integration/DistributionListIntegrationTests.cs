using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
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

    [Fact]
    public async Task GetDistributionLists_ReturnsSuccess()
    {
        using HttpClient client = factory.CreateClient();

        var response = await client.GetAsync("/api/distribution-lists", TestContext.Current.CancellationToken);

        // TODO: Handle authentication and verify response content
        Assert.True(response.IsSuccessStatusCode);
    }
}
