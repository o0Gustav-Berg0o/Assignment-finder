using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.AspNetCore.Identity;

namespace AssignmentFinder.Api.Endpoints.Admin;

public record ApproveUserRequest(string Id);

public class ApproveUserEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<ApproveUserRequest>
{
    public override void Configure()
    {
        Post("/api/admin/users/{Id}/approve");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(ApproveUserRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(req.Id);
        if (user is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        user.IsApproved = true;
        await userManager.UpdateAsync(user);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
