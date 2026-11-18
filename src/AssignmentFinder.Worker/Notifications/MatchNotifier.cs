using AssignmentFinder.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentFinder.Worker.Notifications;

/// <summary>
/// Sends one digest email per user for all of their not-yet-notified matches, then marks
/// those matches as notified. Runs after each ScanPipeline cycle.
/// </summary>
public class MatchNotifier(AppDbContext db, EmailSender emailSender, ILogger<MatchNotifier> logger)
{
    public async Task NotifyAsync(CancellationToken ct)
    {
        var pendingMatches = await db.Matches
            .Where(m => m.NotifiedAt == null)
            .Include(m => m.Assignment)
            .ThenInclude(a => a!.Broker)
            .ToListAsync(ct);

        if (pendingMatches.Count == 0)
        {
            return;
        }

        foreach (var group in pendingMatches.GroupBy(m => m.UserId))
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == group.Key, ct);
            if (user?.Email is null)
            {
                continue;
            }

            var bodyHtml = BuildDigestHtml(group);

            try
            {
                await emailSender.SendAsync(user.Email, $"{group.Count()} new assignment match(es)", bodyHtml, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed sending match digest email to {Email}", user.Email);
                continue;
            }

            foreach (var match in group)
            {
                match.NotifiedAt = DateTimeOffset.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static string BuildDigestHtml(IEnumerable<AssignmentFinder.Domain.Match> matches)
    {
        var items = matches.Select(m =>
            $"<li><a href=\"{m.Assignment!.Url}\">{m.Assignment.Title}</a> - {m.Assignment.Broker!.Name}" +
            (string.IsNullOrWhiteSpace(m.Assignment.Area) ? "" : $" ({m.Assignment.Area})") +
            "</li>");

        return $"<p>New assignment matches:</p><ul>{string.Join("", items)}</ul>";
    }
}
