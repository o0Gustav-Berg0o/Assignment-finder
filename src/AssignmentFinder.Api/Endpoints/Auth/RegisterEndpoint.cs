using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AssignmentFinder.Api.Endpoints.Auth;

public record RegisterRequest(string Email, string Password);

public record RegisterResponse(string UserId, string Email, bool IsApproved);

public class RegisterRequestValidator : Validator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class RegisterEndpoint(UserManager<ApplicationUser> userManager) : Endpoint<RegisterRequest, RegisterResponse>
{
    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        // New self-registrations are never auto-approved; an admin must approve them before
        // they can log in. The bootstrap admin account is seeded separately (see AdminSeeder).
        var user = new ApplicationUser { UserName = req.Email, Email = req.Email, IsApproved = false };
        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Description);
            }
            ThrowIfAnyErrors();
        }

        await HttpContext.Response.SendOkAsync(
            new RegisterResponse(user.Id, user.Email!, user.IsApproved), null, ct);
    }
}
