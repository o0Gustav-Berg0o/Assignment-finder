namespace AssignmentFinder.Domain;

public class Broker
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string WebsiteUrl { get; set; }
    public string? AssignmentPortalUrl { get; set; }

    /// <summary>Key of the registered IAssignmentConnector that knows how to scrape this broker.</summary>
    public required string ConnectorKey { get; set; }

    /// <summary>
    /// For brokers using the "generic-selector" connector: JSON-serialized CssSelectorConfig
    /// describing where to find assignments on AssignmentPortalUrl. Null for brokers with a
    /// bespoke connector class.
    /// </summary>
    public string? SelectorConfigJson { get; set; }

    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset? LastScrapedAt { get; set; }

    public List<Assignment> Assignments { get; set; } = [];
}
