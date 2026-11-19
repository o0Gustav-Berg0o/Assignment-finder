using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Matches;

public record ListMatchesRequest(
    string? Query = null,
    string? Area = null,
    bool UnseenOnly = false,
    int Page = 1,
    int PageSize = 20);

public record MatchResponse(
    int Id, int AssignmentId, string Title, string Url, string? Area, string? Skills,
    string BrokerName, DateTimeOffset MatchedAt, bool IsSeen);

public record ListMatchesResponse(List<MatchResponse> Items, int TotalCount);

public class ListMatchesEndpoint(AppDbContext db) : Endpoint<ListMatchesRequest, ListMatchesResponse>
{
    public override void Configure()
    {
        Get("/api/matches");
    }

    public override async Task HandleAsync(ListMatchesRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var query = db.Matches
            .Where(m => m.UserId == userId)
            .Where(m => !req.UnseenOnly || !m.IsSeen);

        if (!string.IsNullOrWhiteSpace(req.Query))
        {
            query = query.Where(m =>
                m.Assignment!.Title.Contains(req.Query) ||
                (m.Assignment.Description != null && m.Assignment.Description.Contains(req.Query)) ||
                (m.Assignment.Skills != null && m.Assignment.Skills.Contains(req.Query)));
        }

        if (!string.IsNullOrWhiteSpace(req.Area))
        {
            query = query.Where(m => m.Assignment!.Area != null && m.Assignment.Area.Contains(req.Area));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(m => m.MatchedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(m => new MatchResponse(
                m.Id, m.AssignmentId, m.Assignment!.Title, m.Assignment.Url,
                m.Assignment.Area, m.Assignment.Skills, m.Assignment.Broker!.Name,
                m.MatchedAt, m.IsSeen))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(new ListMatchesResponse(items, totalCount), null, ct);
    }
}
