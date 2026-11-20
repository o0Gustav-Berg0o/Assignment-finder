using AngleSharp;
using AngleSharp.Dom;
using AssignmentFinder.Domain;
using Microsoft.Playwright;

namespace AssignmentFinder.Scraping;

/// <summary>
/// CSS selectors for a broker's login form, for connectors whose listing sits behind a login
/// (see PlaywrightConnectorBase.LoginConfig). All three fields are on the page at Broker.LoginUrl.
/// </summary>
public record PlaywrightLoginConfig(string UsernameSelector, string PasswordSelector, string SubmitSelector);

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

    /// <summary>
    /// Override with the target site's login-form selectors for a broker whose listing requires
    /// login (Broker.LoginUrl/LoginUsername/LoginPasswordEncrypted). Null (the default) means this
    /// connector doesn't log in - the common case for brokers with a public listing.
    /// Inspect the real login page in a browser before filling these in; they're never guessed.
    /// </summary>
    protected virtual PlaywrightLoginConfig? LoginConfig => null;

    protected abstract IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document);

    public virtual async Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, string? decryptedPassword, CancellationToken ct)
    {
        var listingUrl = broker.AssignmentPortalUrl ?? broker.WebsiteUrl;

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        var page = await browser.NewPageAsync();

        if (LoginConfig is { } login)
        {
            if (string.IsNullOrWhiteSpace(broker.LoginUsername) || string.IsNullOrWhiteSpace(decryptedPassword))
            {
                throw new InvalidOperationException(
                    $"Connector '{Key}' requires login, but broker '{broker.Name}' has no login username/password configured.");
            }

            await page.GotoAsync(broker.LoginUrl ?? broker.WebsiteUrl);
            await page.FillAsync(login.UsernameSelector, broker.LoginUsername);
            await page.FillAsync(login.PasswordSelector, decryptedPassword);
            await page.ClickAsync(login.SubmitSelector);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await page.GotoAsync(listingUrl);
        await page.WaitForSelectorAsync(ReadySelector, new PageWaitForSelectorOptions { Timeout = 15_000 });

        var html = await page.ContentAsync();
        var document = await browsingContext.OpenAsync(req => req.Content(html), ct);
        return Parse(broker, document);
    }
}
