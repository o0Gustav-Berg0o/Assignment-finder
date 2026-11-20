using AssignmentFinder.Api.Shared;
using AssignmentFinder.Infrastructure.Data;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Assignments;

public record ListAssignmentsRequest(
    string? Query = null,
    string? Area = null,
    int? BrokerId = null,
    bool OnlyInteresting = false,
    int Page = 1,
    int PageSize = 20);

public class ListAssignmentsEndpoint(AppDbContext db) : Endpoint<ListAssignmentsRequest, AssignmentsPageResponse>
{
    public override void Configure()
    {
        Get("/api/assignments");
    }

    public override async Task HandleAsync(ListAssignmentsRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;

        var interestingAssignmentIds = db.AssignmentInterests
            .Where(i => i.UserId == userId)
            .Select(i => i.AssignmentId);

        var query = db.Assignments.AsQueryable().ApplyTextSearch(req.Query, req.Area);

        if (req.BrokerId.HasValue)
        {
            query = query.Where(a => a.BrokerId == req.BrokerId.Value);
        }

        if (req.OnlyInteresting)
        {
            query = query.Where(a => interestingAssignmentIds.Contains(a.Id));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.FirstSeenAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new AssignmentResponse(
                a.Id, a.Title, a.Description, a.Area, a.Skills, a.Url, a.Broker!.Name, a.FirstSeenAt,
                interestingAssignmentIds.Contains(a.Id)))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(new AssignmentsPageResponse(items, totalCount), null, ct);
    }
}
