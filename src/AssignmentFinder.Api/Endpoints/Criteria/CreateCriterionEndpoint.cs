using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using FluentValidation;

namespace AssignmentFinder.Api.Endpoints.Criteria;

public class UpsertCriterionRequestValidator : Validator<UpsertCriterionRequest>
{
    public UpsertCriterionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.MinRequirementMatches).GreaterThanOrEqualTo(0);
    }
}

public class CreateCriterionEndpoint(AppDbContext db) : Endpoint<UpsertCriterionRequest, CriterionResponse>
{
    public override void Configure()
    {
        Post("/api/criteria");
    }

    public override async Task HandleAsync(UpsertCriterionRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var criterion = new SearchCriterion
        {
            UserId = userId,
            Name = req.Name,
            AreaKeywords = req.AreaKeywords,
            RequirementKeywords = req.RequirementKeywords,
            MinRequirementMatches = req.MinRequirementMatches,
            IsActive = req.IsActive,
        };

        db.SearchCriteria.Add(criterion);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendOkAsync(new CriterionResponse(
            criterion.Id, criterion.Name, criterion.AreaKeywords, criterion.RequirementKeywords,
            criterion.MinRequirementMatches, criterion.IsActive), null, ct);
    }
}
