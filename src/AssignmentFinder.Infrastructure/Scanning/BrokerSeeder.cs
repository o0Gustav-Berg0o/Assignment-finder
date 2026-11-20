using System.Text.Json;
using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Scraping;
using AssignmentFinder.Scraping.Connectors;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Infrastructure.Scanning;

/// <summary>
/// Ensures the built-in set of real broker connectors has a matching Broker row, so a scan
/// finds something to scrape without any manual admin setup. Runs idempotently on every startup:
/// inserts missing brokers by Name, leaves existing ones (and their IsEnabled/LastScrapedAt) alone.
/// </summary>
public class BrokerSeeder(AppDbContext db)
{
    private static readonly IReadOnlyList<Broker> SeedBrokers =
    [
        new Broker
        {
            Name = "Brainville",
            WebsiteUrl = "https://www.brainville.com/en/",
            AssignmentPortalUrl = "https://www.brainville.com/en/RequisitionSearch",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = JsonSerializer.Serialize(new CssSelectorConfig
            {
                ItemSelector = "li.c_card",
                TitleSelector = "h3 a",
                LinkSelector = "h3 a",
                AreaSelector = "div[data-flex] span:nth-of-type(2)",
            }),
        },
        new Broker
        {
            Name = "Cinode Market",
            WebsiteUrl = "https://cinode.com/market",
            AssignmentPortalUrl = "https://cinode.com/market",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = JsonSerializer.Serialize(new CssSelectorConfig
            {
                ItemSelector = "div.requests-list__card",
                TitleSelector = "a.list__heading",
                LinkSelector = "a.list__heading",
                AreaSelector = ".focus__item:has(use[href='#icon-map-pin']) p",
                FetchDetailPage = true,
                DetailDescriptionSelector = "article.split__left .wysiwyg",
            }),
        },
        new Broker
        {
            Name = "TED - Offentliga upphandlingar (IT)",
            WebsiteUrl = "https://ted.europa.eu/",
            AssignmentPortalUrl = null,
            ConnectorKey = TedProcurementConnector.ConnectorKey,
            SelectorConfigJson = null,
        },

        // Login-gated candidates: assignment listings require a registered account, so these
        // start disabled. An admin can set LoginUrl/credentials via the Brokers page's quick-login
        // link to inspect the logged-in listing page - but GenericSelectorConnector (plain HTTP, no
        // JS, no login) can never actually use those credentials. Enabling one of these for real
        // scraping means writing a bespoke Playwright connector for it (copy ExampleSpaBrokerConnector,
        // which now supports a LoginConfig step) with real selectors verified against the live
        // logged-in page, then pointing ConnectorKey at that new class.
        new Broker
        {
            Name = "Verama",
            WebsiteUrl = "https://www.verama.com/",
            AssignmentPortalUrl = null,
            LoginUrl = "https://www.verama.com/login",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = null,
            IsEnabled = false,
        },
        new Broker
        {
            Name = "Nexer Xtended",
            WebsiteUrl = "https://nexergroup.com/nexer-xtended/",
            AssignmentPortalUrl = null,
            LoginUrl = "https://nexergroup.com/nexer-xtended/",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = null,
            IsEnabled = false,
        },
        new Broker
        {
            Name = "Onsiter",
            WebsiteUrl = "https://onsiter.com/",
            AssignmentPortalUrl = null,
            LoginUrl = "https://onsiter.com/",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = null,
            IsEnabled = false,
        },
        new Broker
        {
            Name = "Mercell (offentlig upphandling)",
            WebsiteUrl = "https://www.mercell.com/sv-se/",
            AssignmentPortalUrl = null,
            LoginUrl = "https://app.mercell.com/",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = null,
            IsEnabled = false,
        },
        new Broker
        {
            Name = "e-Avrop (offentlig upphandling)",
            WebsiteUrl = "https://www.e-avrop.com/",
            AssignmentPortalUrl = null,
            LoginUrl = "https://www.e-avrop.com/Login.aspx",
            ConnectorKey = GenericSelectorConnector.ConnectorKey,
            SelectorConfigJson = null,
            IsEnabled = false,
        },
    ];

    public async Task SeedAsync()
    {
        var existingNames = await db.Brokers.Select(b => b.Name).ToListAsync();

        foreach (var broker in SeedBrokers)
        {
            if (!existingNames.Contains(broker.Name))
            {
                db.Brokers.Add(broker);
            }
        }

        await db.SaveChangesAsync();
    }
}
