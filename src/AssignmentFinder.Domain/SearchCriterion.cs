namespace AssignmentFinder.Domain;

public class SearchCriterion
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public required string Name { get; set; }

    /// <summary>Comma-separated area keywords, e.g. "Stockholm,Remote".</summary>
    public required string AreaKeywords { get; set; }

    /// <summary>Comma-separated requirement/skill keywords, e.g. "C#,.NET,Azure".</summary>
    public required string RequirementKeywords { get; set; }

    /// <summary>Minimum number of requirement keywords that must match (default: any one).</summary>
    public int MinRequirementMatches { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public List<Match> Matches { get; set; } = [];
}
