using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Matching;
using AssignmentFinder.Scraping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentFinder.Infrastructure.Scanning;

/// <summary>
/// Orchestrates one scrape-and-match cycle: for each enabled broker, resolve its connector,
/// fetch current assignments, upsert them (deduped by BrokerId+ExternalId), then run matching
/// for all active search criteria against newly-seen assignments and record Match rows.
/// </summary>
public class ScanPipeline(
    AppDbContext db,
    IEnumerable<IAssignmentConnector> connectors,
    MatchingService matchingService,
    ILogger<ScanPipeline> logger)
{
    public async Task<ScanResult> RunAsync(CancellationToken ct)
    {
        var brokers = await db.Brokers.Where(b => b.IsEnabled).ToListAsync(ct);
        var newAssignmentIds = new List<int>();

        foreach (var broker in brokers)
        {
            newAssignmentIds.AddRange(await ScanBrokerAsync(broker, ct));
        }

        var matchCount = await MatchNewAssignmentsAsync(newAssignmentIds, ct);

        return new ScanResult(brokers.Count, newAssignmentIds.Count, matchCount);
    }

    /// <summary>Runs the matching step for a set of newly-scraped assignment ids (e.g. from ScanBrokerAsync).</summary>
    public Task<int> MatchAsync(IReadOnlyList<int> newAssignmentIds, CancellationToken ct) =>
        MatchNewAssignmentsAsync(newAssignmentIds, ct);

    public async Task<IReadOnlyList<int>> ScanBrokerAsync(Broker broker, CancellationToken ct)
    {
        var connector = connectors.FirstOrDefault(c => c.Key == broker.ConnectorKey);
        if (connector is null)
        {
            logger.LogWarning(
                "No connector registered for broker '{Broker}' (ConnectorKey: {Key})",
                broker.Name, broker.ConnectorKey);
            return [];
        }

        IReadOnlyList<ScrapedAssignment> scraped;
        try
        {
            scraped = await connector.FetchAsync(broker, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed scraping broker '{Broker}'", broker.Name);
            return [];
        }

        var newIds = new List<int>();
        var now = DateTimeOffset.UtcNow;

        foreach (var item in scraped)
        {
            var existing = await db.Assignments
                .FirstOrDefaultAsync(a => a.BrokerId == broker.Id && a.ExternalId == item.ExternalId, ct);

            if (existing is not null)
            {
                existing.LastSeenAt = now;
                existing.Title = item.Title;
                existing.Description = item.Description;
                existing.Area = item.Area;
                existing.Skills = item.Skills;
                continue;
            }

            var assignment = new Assignment
            {
                BrokerId = broker.Id,
                ExternalId = item.ExternalId,
                Title = item.Title,
                Description = item.Description,
                Area = item.Area,
                Skills = item.Skills,
                Url = item.Url,
                PostedAt = item.PostedAt,
                FirstSeenAt = now,
                LastSeenAt = now,
            };
            db.Assignments.Add(assignment);
            await db.SaveChangesAsync(ct);
            newIds.Add(assignment.Id);
        }

        broker.LastScrapedAt = now;
        await db.SaveChangesAsync(ct);

        return newIds;
    }

    private async Task<int> MatchNewAssignmentsAsync(IReadOnlyList<int> newAssignmentIds, CancellationToken ct)
    {
        if (newAssignmentIds.Count == 0)
        {
            return 0;
        }

        var assignments = await db.Assignments
            .Where(a => newAssignmentIds.Contains(a.Id))
            .ToListAsync(ct);

        var criteria = await db.SearchCriteria.Where(c => c.IsActive).ToListAsync(ct);

        var matches = new List<Match>();
        foreach (var assignment in assignments)
        {
            foreach (var criterion in criteria)
            {
                if (matchingService.IsMatch(assignment, criterion))
                {
                    matches.Add(new Match
                    {
                        UserId = criterion.UserId,
                        AssignmentId = assignment.Id,
                        SearchCriterionId = criterion.Id,
                    });
                }
            }
        }

        if (matches.Count > 0)
        {
            db.Matches.AddRange(matches);
            await db.SaveChangesAsync(ct);
        }

        return matches.Count;
    }
}

public record ScanResult(int BrokersScanned, int NewAssignments, int NewMatches);
