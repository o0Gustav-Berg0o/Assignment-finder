using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Admin;

public record AdminUserResponse(string Id, string Email, bool IsApproved);

public class ListUsersEndpoint(AppDbContext db) : EndpointWithoutRequest<List<AdminUserResponse>>
{
    public override void Configure()
    {
        Get("/api/admin/users");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await db.Users
            .OrderBy(u => u.IsApproved)
            .ThenBy(u => u.Email)
            .Select(u => new AdminUserResponse(u.Id, u.Email!, u.IsApproved))
            .ToListAsync(ct);

        await HttpContext.Response.SendOkAsync(users, null, ct);
    }
}
