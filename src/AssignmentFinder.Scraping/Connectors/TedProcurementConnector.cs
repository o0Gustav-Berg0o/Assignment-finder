using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssignmentFinder.Domain;

namespace AssignmentFinder.Scraping.Connectors;

/// <summary>
/// Fetches Swedish IT/consulting procurement notices from TED (Tenders Electronic Daily), the EU's
/// official public-procurement database. Unlike the other connectors, this calls TED's own JSON
/// search API directly - no HTML scraping - which is explicitly open for anonymous, automated reuse
/// (see https://docs.ted.europa.eu/api/latest/index.html). These are formal tender notices (often
/// framework agreements with a bid deadline), not "apply now" freelance gigs like Brainville/Cinode.
/// </summary>
public class TedProcurementConnector(IHttpClientFactory httpClientFactory) : IAssignmentConnector
{
    public const string ConnectorKey = "ted-procurement";

    public string Key => ConnectorKey;

    private const string SearchUrl = "https://api.ted.europa.eu/v3/notices/search";

    private readonly HttpClient httpClient = httpClientFactory.CreateClient("AssignmentFinder.Scraping");

    // TED mixes casing for language-keyed object properties ("swe"/"eng" on most fields, but
    // "SWE"/"ENG" under links.html) - case-insensitive matching avoids needing two DTO shapes.
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<ScrapedAssignment>> FetchAsync(Broker broker, CancellationToken ct)
    {
        var requestBody = new
        {
            query = "classification-cpv=72000000* AND place-of-performance=SWE AND publication-date>=today(-60)",
            fields = new[]
            {
                "ND", "publication-date", "notice-title", "buyer-name",
                "deadline-receipt-tender-date-lot", "description-proc", "links",
            },
            scope = "ACTIVE",
            onlyLatestVersions = true,
            page = 1,
            limit = 100,
        };

        using var response = await httpClient.PostAsJsonAsync(SearchUrl, requestBody, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TedSearchResponse>(JsonOptions, ct);

        var results = new List<ScrapedAssignment>();
        foreach (var notice in result?.Notices ?? [])
        {
            var title = notice.NoticeTitle?.Swe ?? notice.NoticeTitle?.Eng;
            var url = notice.Links?.Html?.Swe ?? notice.Links?.Html?.Eng;
            if (string.IsNullOrWhiteSpace(notice.Nd) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var buyer = notice.BuyerName?.Swe?.FirstOrDefault();
            var deadline = notice.DeadlineReceiptTenderDateLot?.FirstOrDefault();
            var description = notice.DescriptionProc?.Swe;

            var fullDescription = deadline is null
                ? description
                : $"Sista anbudsdag: {deadline}\n\n{description}";

            DateTimeOffset.TryParse(notice.PublicationDate, out var postedAt);

            results.Add(new ScrapedAssignment(
                ExternalId: notice.Nd,
                Title: title,
                Url: url,
                Description: fullDescription,
                Area: buyer,
                PostedAt: postedAt == default ? null : postedAt));
        }

        return results;
    }

    private record TedSearchResponse(
        [property: JsonPropertyName("notices")] List<TedNotice>? Notices);

    private record TedNotice(
        [property: JsonPropertyName("ND")] string? Nd,
        [property: JsonPropertyName("publication-date")] string? PublicationDate,
        [property: JsonPropertyName("notice-title")] TedLocalizedText? NoticeTitle,
        [property: JsonPropertyName("buyer-name")] TedLocalizedList? BuyerName,
        [property: JsonPropertyName("description-proc")] TedLocalizedText? DescriptionProc,
        [property: JsonPropertyName("deadline-receipt-tender-date-lot")] List<string>? DeadlineReceiptTenderDateLot,
        [property: JsonPropertyName("links")] TedLinks? Links);

    private record TedLocalizedText(
        [property: JsonPropertyName("swe")] string? Swe,
        [property: JsonPropertyName("eng")] string? Eng);

    private record TedLocalizedList(
        [property: JsonPropertyName("swe")] List<string>? Swe);

    private record TedLinks(
        [property: JsonPropertyName("html")] TedLocalizedText? Html);
}
