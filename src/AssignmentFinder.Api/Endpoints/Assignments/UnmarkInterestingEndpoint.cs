using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Assignments;

public record UnmarkInterestingRequest(int Id);

public class UnmarkInterestingEndpoint(AppDbContext db) : Endpoint<UnmarkInterestingRequest>
{
    public override void Configure()
    {
        Delete("/api/assignments/{Id}/interesting");
    }

    public override async Task HandleAsync(UnmarkInterestingRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var interest = await db.AssignmentInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.AssignmentId == req.Id, ct);

        if (interest is not null)
        {
            db.AssignmentInterests.Remove(interest);
            await db.SaveChangesAsync(ct);
        }

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
