using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record BrokerResponse(
    int Id, string Name, string WebsiteUrl, string? AssignmentPortalUrl,
    string ConnectorKey, bool IsEnabled, DateTimeOffset? LastScrapedAt);

public class ListBrokersEndpoint(AppDbContext db) : EndpointWithoutRequest<List<BrokerResponse>>
{
    public override void Configure()
    {
        Get("/api/brokers");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var brokers = await db.Brokers
            .Select(b => new BrokerResponse(
                b.Id, b.Name, b.WebsiteUrl, b.AssignmentPortalUrl, b.ConnectorKey, b.IsEnabled, b.LastScrapedAt))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(brokers, null, ct);
    }
}
