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
