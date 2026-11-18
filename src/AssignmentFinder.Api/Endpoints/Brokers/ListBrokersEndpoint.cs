using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record BrokerResponse(
    int Id, string Name, string WebsiteUrl, string? AssignmentPortalUrl,
    string ConnectorKey, string? SelectorConfigJson, bool IsEnabled, DateTimeOffset? LastScrapedAt,
    bool? LastScanSucceeded, string? LastScanError, int AssignmentCount, bool HasLoginCredentials,
    string? LoginUsername);

public class ListBrokersEndpoint(AppDbContext db) : EndpointWithoutRequest<List<BrokerResponse>>
{
    public override void Configure()
    {
        Get("/api/brokers");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var brokers = await db.Brokers
            .Select(b => new BrokerResponse(
                b.Id, b.Name, b.WebsiteUrl, b.AssignmentPortalUrl, b.ConnectorKey, b.SelectorConfigJson,
                b.IsEnabled, b.LastScrapedAt, b.LastScanSucceeded, b.LastScanError, b.Assignments.Count,
                b.LoginPasswordEncrypted != null, b.LoginUsername))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(brokers, null, ct);
    }
}
