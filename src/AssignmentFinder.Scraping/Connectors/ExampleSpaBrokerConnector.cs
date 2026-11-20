using AngleSharp.Dom;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping.Connectors;

/// <summary>
/// Template connector for a broker whose assignment list is a JS-rendered SPA (Angular/React),
/// so a headless browser is needed to see the real DOM. Copy this class, rename it, point
/// ReadySelector/Parse at the target site's real markup (inspect it in a browser first), then
/// add a Broker row with ConnectorKey = this class's Key.
///
/// If the listing requires login (Broker.LoginUrl/LoginUsername/LoginPassword, e.g. Verama,
/// Onsiter, Nexer Xtended, Mercell, e-Avrop), also override LoginConfig with that site's real
/// login-form selectors - PlaywrightConnectorBase logs in before navigating to the listing.
/// The generic-selector connector (plain HTTP, no JS) can never do this; a login-gated broker
/// needs its own class like this one.
///
/// Disabled by default: the selectors below are illustrative placeholders, not verified against
/// a live site. Verify and adjust before enabling the corresponding Broker.
/// </summary>
public class ExampleSpaBrokerConnector : PlaywrightConnectorBase
{
    public const string ConnectorKey = "example-spa-broker";

    public override string Key => ConnectorKey;

    protected override string ReadySelector => "[data-testid='assignment-list']";

    protected override IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document)
    {
        var results = new List<ScrapedAssignment>();

        foreach (var item in document.QuerySelectorAll("[data-testid='assignment-card']"))
        {
            var link = item.QuerySelector("a");
            var href = link?.GetAttribute("href");
            var title = item.QuerySelector("h3")?.TextContent.Trim();

            if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            // A root-relative path like "/en/x?y=1" parses as a valid absolute file:// URI on
            // Linux, so TryCreate alone isn't enough - only trust it for actual http(s) URLs.
            var url = Uri.TryCreate(href, UriKind.Absolute, out var absolute) &&
                      (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps)
                ? absolute.ToString()
                : new Uri(new Uri(broker.AssignmentPortalUrl ?? broker.WebsiteUrl), href).ToString();

            results.Add(new ScrapedAssignment(
                ExternalId: url,
                Title: title,
                Url: url,
                Area: item.QuerySelector("[data-testid='assignment-location']")?.TextContent.Trim(),
                Skills: item.QuerySelector("[data-testid='assignment-skills']")?.TextContent.Trim()));
        }

        return results;
    }
}
