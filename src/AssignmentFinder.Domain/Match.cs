namespace AssignmentFinder.Domain;

public class Match
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public int SearchCriterionId { get; set; }
    public SearchCriterion? SearchCriterion { get; set; }

    public DateTimeOffset MatchedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? NotifiedAt { get; set; }
    public bool IsSeen { get; set; }
}
