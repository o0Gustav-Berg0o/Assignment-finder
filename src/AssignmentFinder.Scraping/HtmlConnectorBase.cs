using AngleSharp;
using AngleSharp.Dom;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping;

/// <summary>Base for connectors that scrape server-rendered HTML (no JS execution needed).</summary>
public abstract class HtmlConnectorBase(IHttpClientFactory httpClientFactory) : IAssignmentConnector
{
    private static readonly IBrowsingContext browsingContext =
        AngleSharp.BrowsingContext.New(Configuration.Default);

    /// <summary>Shared HttpClient (politeness User-Agent/timeout configured centrally) for any extra
    /// requests a subclass needs beyond the main listing page, e.g. following a detail-page link.</summary>
    protected readonly HttpClient httpClient = httpClientFactory.CreateClient("AssignmentFinder.Scraping");

    public abstract string Key { get; }

    /// <summary>Parse the loaded listing document into normalized assignments.</summary>
    protected abstract IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document);

    protected static Task<IDocument> ParseHtmlAsync(string html, CancellationToken ct) =>
        browsingContext.OpenAsync(req => req.Content(html), ct);

    public virtual async Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, string? decryptedPassword, CancellationToken ct)
    {
        var listingUrl = broker.AssignmentPortalUrl ?? broker.WebsiteUrl;
        var html = await httpClient.GetStringAsync(listingUrl, ct);
        var document = await ParseHtmlAsync(html, ct);
        return Parse(broker, document);
    }
}
