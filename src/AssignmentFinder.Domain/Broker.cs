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

    /// <summary>Whether the most recent scan completed without error. Null before the first scan.</summary>
    public bool? LastScanSucceeded { get; set; }

    /// <summary>Exception message from the most recent failed scan, if any.</summary>
    public string? LastScanError { get; set; }

    /// <summary>Username for brokers whose listing requires login. Null if none is needed.</summary>
    public string? LoginUsername { get; set; }

    /// <summary>Password for LoginUsername, encrypted at rest via BrokerCredentialProtector.</summary>
    public string? LoginPasswordEncrypted { get; set; }

    /// <summary>The site's own login page, if different from WebsiteUrl. Used for a quick "log in" link.</summary>
    public string? LoginUrl { get; set; }

    public List<Assignment> Assignments { get; set; } = [];
}
