using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Assignments;

public record MarkInterestingRequest(int Id);

public class MarkInterestingEndpoint(AppDbContext db) : Endpoint<MarkInterestingRequest>
{
    public override void Configure()
    {
        Post("/api/assignments/{Id}/interesting");
    }

    public override async Task HandleAsync(MarkInterestingRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var assignmentExists = await db.Assignments.AnyAsync(a => a.Id == req.Id, ct);
        if (!assignmentExists)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        var alreadyMarked = await db.AssignmentInterests
            .AnyAsync(i => i.UserId == userId && i.AssignmentId == req.Id, ct);

        if (!alreadyMarked)
        {
            db.AssignmentInterests.Add(new AssignmentInterest { UserId = userId, AssignmentId = req.Id });
            await db.SaveChangesAsync(ct);
        }

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
