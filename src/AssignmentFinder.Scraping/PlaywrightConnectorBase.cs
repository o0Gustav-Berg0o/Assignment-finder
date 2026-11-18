using AngleSharp;
using AngleSharp.Dom;
using AssignmentFinder.Domain;
using Microsoft.Playwright;

namespace AssignmentFinder.Scraping;

/// <summary>
/// Base for connectors that need a headless browser because the listing page is rendered
/// client-side (Angular/React SPA). Renders the page with Playwright, then parses the resulting
/// HTML with AngleSharp so concrete connectors can reuse the same selector-based Parse logic.
/// </summary>
public abstract class PlaywrightConnectorBase : IAssignmentConnector
{
    private static readonly IBrowsingContext browsingContext =
        AngleSharp.BrowsingContext.New(Configuration.Default);

    public abstract string Key { get; }

    /// <summary>CSS selector to wait for before considering the page fully rendered.</summary>
    protected abstract string ReadySelector { get; }

    protected abstract IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document);

    public virtual async Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, CancellationToken ct)
    {
        var listingUrl = broker.AssignmentPortalUrl ?? broker.WebsiteUrl;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        var page = await browser.NewPageAsync();
        await page.GotoAsync(listingUrl);
        await page.WaitForSelectorAsync(ReadySelector, new PageWaitForSelectorOptions { Timeout = 15_000 });

        var html = await page.ContentAsync();
        var document = await browsingContext.OpenAsync(req => req.Content(html), ct);
        return Parse(broker, document);
    }
}
