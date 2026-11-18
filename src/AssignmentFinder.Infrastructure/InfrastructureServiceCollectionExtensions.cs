using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Matching;
using AssignmentFinder.Infrastructure.Scanning;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentFinder.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAssignmentFinderInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Default")));

        // Persisted to the DB (not the container filesystem, which is wiped on every restart/
        // redeploy) and given a fixed application name so the api and worker containers - which
        // each run their own process - decrypt data protected by either of them.
        services.AddDataProtection()
            .SetApplicationName("AssignmentFinder")
            .PersistKeysToDbContext<AppDbContext>();
        services.AddScoped<BrokerCredentialProtector>();

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                // Relaxed so a plain lowercase GUID (128 bits of entropy, no uppercase letters)
                // is accepted - length/randomness matter more here than character-class mixing.
                options.Password.RequireUppercase = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<MatchingService>();
        services.AddScoped<ScanPipeline>();
        services.AddScoped<AdminSeeder>();
        services.AddScoped<BrokerSeeder>();

        return services;
    }
}
