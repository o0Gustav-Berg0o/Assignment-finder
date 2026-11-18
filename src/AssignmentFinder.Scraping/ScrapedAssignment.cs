namespace AssignmentFinder.Scraping;

/// <summary>Normalized assignment data a connector extracts from a broker's site, before persistence.</summary>
public record ScrapedAssignment(
    string ExternalId,
    string Title,
    string Url,
    string? Description = null,
    string? Area = null,
    string? Skills = null,
    DateTimeOffset? PostedAt = null);
