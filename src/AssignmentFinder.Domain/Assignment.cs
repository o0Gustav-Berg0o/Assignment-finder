namespace AssignmentFinder.Domain;

public class Assignment
{
    public int Id { get; set; }

    public int BrokerId { get; set; }
    public Broker? Broker { get; set; }

    /// <summary>Identifier/URL from the source site, used to dedupe across scrape runs.</summary>
    public required string ExternalId { get; set; }

    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? Area { get; set; }

    /// <summary>Free-text skills/requirements as found on the source page.</summary>
    public string? Skills { get; set; }

    public required string Url { get; set; }
    public DateTimeOffset? PostedAt { get; set; }

    public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;

    public List<Match> Matches { get; set; } = [];
}
