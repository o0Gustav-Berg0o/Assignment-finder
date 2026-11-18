using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Matches;

public record MarkSeenRequest(int Id);

public class MarkSeenEndpoint(AppDbContext db) : Endpoint<MarkSeenRequest>
{
    public override void Configure()
    {
        Post("/api/matches/{Id}/seen");
    }

    public override async Task HandleAsync(MarkSeenRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var match = await db.Matches.FirstOrDefaultAsync(m => m.Id == req.Id && m.UserId == userId, ct);
        if (match is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        match.IsSeen = true;
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
