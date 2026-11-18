using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record DeleteBrokerRequest(int Id);

public class DeleteBrokerEndpoint(AppDbContext db) : Endpoint<DeleteBrokerRequest>
{
    public override void Configure()
    {
        Delete("/api/brokers/{Id}");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(DeleteBrokerRequest req, CancellationToken ct)
    {
        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Id == req.Id, ct);
        if (broker is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        db.Brokers.Remove(broker);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendNoContentAsync(ct);
    }
}
