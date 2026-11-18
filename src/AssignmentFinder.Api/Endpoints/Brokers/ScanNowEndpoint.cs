using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Scanning;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record ScanNowRequest(int Id);

public record ScanNowResponse(int NewAssignments, int NewMatches);

public class ScanNowEndpoint(AppDbContext db, ScanPipeline pipeline) : Endpoint<ScanNowRequest, ScanNowResponse>
{
    public override void Configure()
    {
        Post("/api/brokers/{Id}/scan-now");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(ScanNowRequest req, CancellationToken ct)
    {
        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Id == req.Id, ct);
        if (broker is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        var newIds = await pipeline.ScanBrokerAsync(broker, ct);
        var matchCount = await pipeline.MatchAsync(newIds, ct);

        await HttpContext.Response.SendOkAsync(new ScanNowResponse(newIds.Count, matchCount), null, ct);
    }
}
