using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping;

/// <summary>
/// Knows how to fetch the current list of open assignments from one broker/marketplace site.
/// Register one implementation per broker "shape"; <see cref="Key"/> must match Broker.ConnectorKey.
/// Several Broker rows can share the same generic connector (see GenericSelectorConnector) when
/// their listing page has simple, config-describable markup; sites with bespoke/JS-heavy markup
/// get their own dedicated connector class instead.
/// </summary>
public interface IAssignmentConnector
{
    /// <summary>Stable identifier matching Broker.ConnectorKey, e.g. "generic-selector" or "acme-broker".</summary>
    string Key { get; }

    /// <summary>
    /// Fetches the broker's current assignment list. <paramref name="decryptedPassword"/> is the
    /// plaintext form of Broker.LoginPasswordEncrypted (decrypted by the caller, which holds the
    /// protector - Scraping has no dependency on Infrastructure), or null if the broker has no
    /// stored password. Connectors that don't need login (most of them) simply ignore it.
    /// </summary>
    Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, string? decryptedPassword, CancellationToken ct);
}
