namespace AssignmentFinder.Api.Endpoints.Brokers;

public record UpsertBrokerRequest(
    string Name,
    string WebsiteUrl,
    string? AssignmentPortalUrl,
    string ConnectorKey,
    string? SelectorConfigJson,
    bool IsEnabled,
    string? LoginUsername,
    string? LoginPassword);

public record UpdateBrokerRequest(
    int Id,
    string Name,
    string WebsiteUrl,
    string? AssignmentPortalUrl,
    string ConnectorKey,
    string? SelectorConfigJson,
    bool IsEnabled,
    /// <summary>Empty/null clears any stored login credentials.</summary>
    string? LoginUsername,
    /// <summary>Empty/null when LoginUsername is unchanged keeps the existing password; set to change it.</summary>
    string? LoginPassword);
