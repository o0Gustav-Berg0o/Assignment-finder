using AssignmentFinder.Infrastructure.Scanning;
using AssignmentFinder.Worker.Notifications;
using Microsoft.Extensions.Logging;
using Quartz;

namespace AssignmentFinder.Worker.Jobs;

/// <summary>Scheduled job: scrape all enabled brokers, match, then email digests for new matches.</summary>
[DisallowConcurrentExecution]
public class ScanJob(ScanPipeline pipeline, MatchNotifier notifier, ILogger<ScanJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken ct)
    {
        logger.LogInformation("Starting scheduled scan");
        var result = await pipeline.RunAsync(ct);
        logger.LogInformation(
            "Scan complete: {Brokers} brokers, {NewAssignments} new assignments, {NewMatches} new matches",
            result.BrokersScanned, result.NewAssignments, result.NewMatches);

        await notifier.NotifyAsync(ct);
    }
}
