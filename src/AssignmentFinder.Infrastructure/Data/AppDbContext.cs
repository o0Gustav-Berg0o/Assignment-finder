using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SearchCriterion> SearchCriteria => Set<SearchCriterion>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Broker>(b =>
        {
            b.HasIndex(x => x.ConnectorKey).IsUnique();
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
    }
}
