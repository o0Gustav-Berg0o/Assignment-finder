using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record SetBrokerEnabledRequest(int Id, bool IsEnabled);

public class SetBrokerEnabledEndpoint(AppDbContext db) : Endpoint<SetBrokerEnabledRequest>
{
    public override void Configure()
    {
        Post("/api/brokers/{Id}/enabled");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(SetBrokerEnabledRequest req, CancellationToken ct)
    {
        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Id == req.Id, ct);
        if (broker is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        broker.IsEnabled = req.IsEnabled;
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
