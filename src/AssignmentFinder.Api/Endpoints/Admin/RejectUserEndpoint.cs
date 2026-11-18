using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace AssignmentFinder.Api.Endpoints.Admin;

public record RejectUserRequest(string Id);

/// <summary>
/// Deletes a pending (not yet approved) registration. Only works on unapproved accounts, so
/// this can't be used to accidentally remove someone who's already been let in.
/// </summary>
public class RejectUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<RejectUserRequest>
{
    public override void Configure()
    {
        Post("/api/admin/users/{Id}/reject");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(RejectUserRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.Id);
        if (user is null || user.IsApproved)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        await userManager.DeleteAsync(user);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
