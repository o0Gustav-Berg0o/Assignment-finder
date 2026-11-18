using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Criteria;

public class ListCriteriaEndpoint(AppDbContext db) : EndpointWithoutRequest<List<CriterionResponse>>
{
    public override void Configure()
    {
        Get("/api/criteria");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var criteria = await db.SearchCriteria
            .Where(c => c.UserId == userId)
            .Select(c => new CriterionResponse(
                c.Id, c.Name, c.AreaKeywords, c.RequirementKeywords, c.MinRequirementMatches, c.IsActive))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(criteria, null, ct);
    }
}
