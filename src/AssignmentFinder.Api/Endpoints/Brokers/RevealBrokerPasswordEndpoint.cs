using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Scanning;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record RevealBrokerPasswordRequest(int Id);

public record RevealBrokerPasswordResponse(string Password);

/// <summary>Decrypts and returns a broker's stored login password, for the admin to use when logging in manually.</summary>
public class RevealBrokerPasswordEndpoint(AppDbContext db, BrokerCredentialProtector credentialProtector)
    : Endpoint<RevealBrokerPasswordRequest, RevealBrokerPasswordResponse>
{
    public override void Configure()
    {
        Get("/api/brokers/{Id}/login-password");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(RevealBrokerPasswordRequest req, CancellationToken ct)
    {
        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Id == req.Id, ct);
        if (broker?.LoginPasswordEncrypted is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        var password = credentialProtector.Decrypt(broker.LoginPasswordEncrypted);
        await HttpContext.Response.SendOkAsync(new RevealBrokerPasswordResponse(password), null, ct);
    }
}
