using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Matching;
using AssignmentFinder.Infrastructure.Scanning;
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

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<MatchingService>();
        services.AddScoped<ScanPipeline>();
        services.AddScoped<AdminSeeder>();

        return services;
    }
}
