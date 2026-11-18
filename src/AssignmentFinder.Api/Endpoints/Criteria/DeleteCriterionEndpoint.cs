using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Criteria;

public record DeleteCriterionRequest(int Id);

public class DeleteCriterionEndpoint(AppDbContext db) : Endpoint<DeleteCriterionRequest>
{
    public override void Configure()
    {
        Delete("/api/criteria/{Id}");
    }

    public override async Task HandleAsync(DeleteCriterionRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var criterion = await db.SearchCriteria
            .FirstOrDefaultAsync(c => c.Id == req.Id && c.UserId == userId, ct);

        if (criterion is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        db.SearchCriteria.Remove(criterion);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
