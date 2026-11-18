namespace AssignmentFinder.Api.Endpoints.Criteria;

public record CriterionResponse(
    int Id,
    string Name,
    string AreaKeywords,
    string RequirementKeywords,
    int MinRequirementMatches,
    bool IsActive);

public record UpsertCriterionRequest(
    string Name,
    string AreaKeywords,
    string RequirementKeywords,
    int MinRequirementMatches = 1,
    bool IsActive = true);
