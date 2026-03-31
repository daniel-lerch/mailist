using Mailist.EmailDelivery.Entities;
using Mailist.EmailRelay.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mailist;

/// <summary>
/// Central entrypoint for Mailist's entire database.
/// </summary>
/// <remarks>
/// Using multiple DbContexts on a single database would come with major drawbacks:<br />
/// https://stackoverflow.com/a/11198345/7075733
/// </remarks>
public sealed class DatabaseContext : DbContext
{
    public DbSet<InboxEmail> InboxEmails => Set<InboxEmail>();
    public DbSet<DistributionList> DistributionLists => Set<DistributionList>();

    public DbSet<OutboxEmail> OutboxEmails => Set<OutboxEmail>();
    public DbSet<SentEmail> SentEmails => Set<SentEmail>();


    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateEmailRelay(modelBuilder);

        CreateEmailDelivery(modelBuilder);
    }

    private void CreateEmailRelay(ModelBuilder modelBuilder)
    {
        var inboxEmail = modelBuilder.Entity<InboxEmail>();
        inboxEmail.HasKey(e => e.Id);
        inboxEmail.HasOne(e => e.DistributionList).WithMany().HasForeignKey(e => e.DistributionListId)
            .OnDelete(DeleteBehavior.SetNull);
        inboxEmail.HasIndex(e => e.UniqueId).IsUnique();
        inboxEmail.HasIndex(e => e.ProcessingCompletedTime);
        inboxEmail.Property(e => e.SpamCategory).HasConversion<int>();
        inboxEmail.Property(e => e.DownloadTime).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        var distributionList = modelBuilder.Entity<DistributionList>();
        distributionList.HasKey(dl => dl.Id);
        distributionList.HasIndex(dl => dl.Alias).IsUnique();
        distributionList.Property(dl => dl.Flags).HasConversion<int>();
    }

    private void CreateEmailDelivery(ModelBuilder modelBuilder)
    {
        var outboxEmail = modelBuilder.Entity<OutboxEmail>();
        outboxEmail.HasKey(e => e.Id);
        outboxEmail.HasOne(e => e.InboxEmail).WithMany().HasForeignKey(e => e.InboxEmailId);

        var sentEmail = modelBuilder.Entity<SentEmail>();
        sentEmail.HasKey(e => e.Id);
        sentEmail.HasOne(e => e.InboxEmail).WithMany().HasForeignKey(e => e.InboxEmailId);
        sentEmail.HasIndex(e => e.DeliveryTime);
        sentEmail.Property(e => e.Id).ValueGeneratedNever();
    }
}
