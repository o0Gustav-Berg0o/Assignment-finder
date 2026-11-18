namespace AssignmentFinder.Domain;

/// <summary>A user manually flagging one assignment (from the full list, not just matches) as interesting.</summary>
public class AssignmentInterest
{
    public int Id { get; set; }

    public required string UserId { get; set; }

    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public DateTimeOffset MarkedAt { get; set; } = DateTimeOffset.UtcNow;
}
