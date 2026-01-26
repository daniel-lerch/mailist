using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading;

namespace Mailist.Tests.Integration;

public class MockChurchToolsApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddTransient(_ =>
            {
                var mock = new Mock<IChurchToolsApi>();
                mock.Setup(api => api.ChurchQuery(It.IsAny<ChurchQueryRequest<IdNameEmail>>(), It.IsAny<CancellationToken>()).Result)
                    .Returns([]);
                return mock.Object;
            });
        });
    }
}
