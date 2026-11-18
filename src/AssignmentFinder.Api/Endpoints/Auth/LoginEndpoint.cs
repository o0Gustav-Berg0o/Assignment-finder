using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using FastEndpoints.Security;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AssignmentFinder.Api.Endpoints.Auth;

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token);

public class LoginRequestValidator : Validator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginEndpoint(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, req.Password))
        {
            AddError("Invalid email or password.");
            ThrowIfAnyErrors();
            return;
        }

        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var token = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = signingKey;
            o.ExpireAt = DateTime.UtcNow.AddDays(7);
            o.User.Claims.Add(new("sub", user.Id));
            o.User.Claims.Add(new("email", user.Email!));
        });

        await HttpContext.Response.SendOkAsync(new LoginResponse(token), null, ct);
    }
}
