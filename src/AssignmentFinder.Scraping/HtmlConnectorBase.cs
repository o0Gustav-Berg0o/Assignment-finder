using AngleSharp;
using AngleSharp.Dom;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping;

/// <summary>Base for connectors that scrape server-rendered HTML (no JS execution needed).</summary>
public abstract class HtmlConnectorBase(IHttpClientFactory httpClientFactory) : IAssignmentConnector
{
    private static readonly IBrowsingContext browsingContext =
        AngleSharp.BrowsingContext.New(Configuration.Default);

    private readonly HttpClient httpClient = httpClientFactory.CreateClient("AssignmentFinder.Scraping");

    public abstract string Key { get; }

    /// <summary>Parse the loaded listing document into normalized assignments.</summary>
    protected abstract IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document);

    public virtual async Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, CancellationToken ct)
    {
        var listingUrl = broker.AssignmentPortalUrl ?? broker.WebsiteUrl;
        var html = await httpClient.GetStringAsync(listingUrl, ct);
        var document = await browsingContext.OpenAsync(req => req.Content(html), ct);
        return Parse(broker, document);
    }
}
