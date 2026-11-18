using AngleSharp.Dom;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping.Connectors;

/// <summary>
/// Template connector for a broker whose assignment list is a JS-rendered SPA (Angular/React),
/// so a headless browser is needed to see the real DOM. Copy this class, rename it, point
/// ReadySelector/Parse at the target site's real markup (inspect it in a browser first), then
/// add a Broker row with ConnectorKey = this class's Key.
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

            var url = Uri.TryCreate(href, UriKind.Absolute, out var absolute)
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
