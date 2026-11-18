using AssignmentFinder.Domain;

namespace AssignmentFinder.Infrastructure.Matching;

/// <summary>
/// Pure matching logic: decides whether an assignment satisfies a search criterion.
/// Kept free of EF/DB concerns so it can be unit tested in isolation.
/// </summary>
public class MatchingService
{
    public bool IsMatch(Assignment assignment, SearchCriterion criterion)
    {
        var areaKeywords = SplitKeywords(criterion.AreaKeywords);
        var requirementKeywords = SplitKeywords(criterion.RequirementKeywords);

        var areaText = assignment.Area ?? string.Empty;
        var requirementText = $"{assignment.Title} {assignment.Description} {assignment.Skills}";

        var areaMatches = areaKeywords.Count == 0
            || areaKeywords.Any(k => areaText.Contains(k, StringComparison.OrdinalIgnoreCase));

        var requirementMatchCount = requirementKeywords
            .Count(k => requirementText.Contains(k, StringComparison.OrdinalIgnoreCase));

        var requirementMatches = requirementKeywords.Count == 0
            || requirementMatchCount >= criterion.MinRequirementMatches;

        return areaMatches && requirementMatches;
    }

    private static List<string> SplitKeywords(string keywords) =>
        keywords
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();
}
