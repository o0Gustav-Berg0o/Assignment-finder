using System.Text.Json;
using AngleSharp.Dom;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping.Connectors;

/// <summary>
/// Scrapes any broker whose listing page has simple, server-rendered markup describable with a
/// handful of CSS selectors (see <see cref="CssSelectorConfig"/>, stored on Broker.SelectorConfigJson).
/// This is the connector for the common case; brokers with JS-rendered SPAs or unusually complex
/// markup get a bespoke connector class instead (see PlaywrightConnectorBase).
/// </summary>
public class GenericSelectorConnector(IHttpClientFactory httpClientFactory) : HtmlConnectorBase(httpClientFactory)
{
    public const string ConnectorKey = "generic-selector";

    public override string Key => ConnectorKey;

    protected override IReadOnlyList<ScrapedAssignment> Parse(Broker broker, IDocument document)
    {
        if (string.IsNullOrWhiteSpace(broker.SelectorConfigJson))
        {
            throw new InvalidOperationException(
                $"Broker '{broker.Name}' uses the generic-selector connector but has no SelectorConfigJson configured.");
        }

        var config = JsonSerializer.Deserialize<CssSelectorConfig>(broker.SelectorConfigJson)
            ?? throw new InvalidOperationException($"Could not parse SelectorConfigJson for broker '{broker.Name}'.");

        var results = new List<ScrapedAssignment>();

        foreach (var item in document.QuerySelectorAll(config.ItemSelector))
        {
            var linkElement = item.QuerySelector(config.LinkSelector);
            var href = linkElement?.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            var url = MakeAbsolute(href, broker);
            var title = item.QuerySelector(config.TitleSelector)?.TextContent.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            results.Add(new ScrapedAssignment(
                ExternalId: url,
                Title: title,
                Url: url,
                Description: GetOptionalText(item, config.DescriptionSelector),
                Area: GetOptionalText(item, config.AreaSelector),
                Skills: GetOptionalText(item, config.SkillsSelector)));
        }

        return results;
    }

    private static string? GetOptionalText(IElement item, string? selector) =>
        string.IsNullOrWhiteSpace(selector) ? null : item.QuerySelector(selector)?.TextContent.Trim();

    private static string MakeAbsolute(string href, Broker broker)
    {
        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var baseUri = new Uri(broker.AssignmentPortalUrl ?? broker.WebsiteUrl);
        return new Uri(baseUri, href).ToString();
    }
}
