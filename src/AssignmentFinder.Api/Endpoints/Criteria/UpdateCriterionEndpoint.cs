using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Criteria;

public record UpdateCriterionRequest(
    int Id, string Name, string AreaKeywords, string RequirementKeywords,
    int MinRequirementMatches, bool IsActive);

public class UpdateCriterionEndpoint(AppDbContext db) : Endpoint<UpdateCriterionRequest, CriterionResponse>
{
    public override void Configure()
    {
        Put("/api/criteria/{Id}");
    }

    public override async Task HandleAsync(UpdateCriterionRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var criterion = await db.SearchCriteria
            .FirstOrDefaultAsync(c => c.Id == req.Id && c.UserId == userId, ct);

        if (criterion is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        criterion.Name = req.Name;
        criterion.AreaKeywords = req.AreaKeywords;
        criterion.RequirementKeywords = req.RequirementKeywords;
        criterion.MinRequirementMatches = req.MinRequirementMatches;
        criterion.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendOkAsync(new CriterionResponse(
            criterion.Id, criterion.Name, criterion.AreaKeywords, criterion.RequirementKeywords,
            criterion.MinRequirementMatches, criterion.IsActive), null, ct);
    }
}
