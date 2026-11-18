using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
{
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SearchCriterion> SearchCriteria => Set<SearchCriterion>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<AssignmentInterest> AssignmentInterests => Set<AssignmentInterest>();
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Broker>(b =>
        {
            // Several brokers legitimately share one connector (e.g. "generic-selector" handles
            // any broker whose listing markup is describable via SelectorConfigJson), so ConnectorKey
            // must not be unique. Name is what actually identifies one broker.
            b.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Assignment>(a =>
        {
            a.HasIndex(x => new { x.BrokerId, x.ExternalId }).IsUnique();
            a.HasOne(x => x.Broker).WithMany(x => x.Assignments).HasForeignKey(x => x.BrokerId);
        });

        builder.Entity<SearchCriterion>(c =>
        {
            c.HasIndex(x => x.UserId);
        });

        builder.Entity<Match>(m =>
        {
            m.HasIndex(x => new { x.AssignmentId, x.SearchCriterionId }).IsUnique();
            m.HasOne(x => x.Assignment).WithMany(x => x.Matches).HasForeignKey(x => x.AssignmentId);
            m.HasOne(x => x.SearchCriterion).WithMany(x => x.Matches).HasForeignKey(x => x.SearchCriterionId);
        });

        builder.Entity<AssignmentInterest>(i =>
        {
            i.HasIndex(x => new { x.UserId, x.AssignmentId }).IsUnique();
            i.HasOne(x => x.Assignment).WithMany().HasForeignKey(x => x.AssignmentId);
        });
    }
}
