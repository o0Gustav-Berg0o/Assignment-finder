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

    Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, CancellationToken ct);
}
