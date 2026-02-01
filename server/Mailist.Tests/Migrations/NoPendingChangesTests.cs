using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mailist.Tests.Migrations;

public class NoPendingChangesTests : DatabaseTestBase
{
    [Fact]
    public void NoPendingChanges()
    {
        Assert.False(databaseContext.Database.HasPendingModelChanges());
    }
}
