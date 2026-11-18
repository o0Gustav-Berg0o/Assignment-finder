using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Scanning;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public class UpdateBrokerRequestValidator : Validator<UpdateBrokerRequest>
{
    public UpdateBrokerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.WebsiteUrl).NotEmpty();
        RuleFor(x => x.ConnectorKey).NotEmpty();
    }
}

public class UpdateBrokerEndpoint(AppDbContext db, BrokerCredentialProtector credentialProtector)
    : Endpoint<UpdateBrokerRequest, BrokerResponse>
{
    public override void Configure()
    {
        Put("/api/brokers/{Id}");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(UpdateBrokerRequest req, CancellationToken ct)
    {
        var broker = await db.Brokers.FirstOrDefaultAsync(b => b.Id == req.Id, ct);
        if (broker is null)
        {
            await HttpContext.Response.SendNotFoundAsync(ct);
            return;
        }

        broker.Name = req.Name;
        broker.WebsiteUrl = req.WebsiteUrl;
        broker.AssignmentPortalUrl = req.AssignmentPortalUrl;
        broker.LoginUrl = req.LoginUrl;
        broker.ConnectorKey = req.ConnectorKey;
        broker.SelectorConfigJson = req.SelectorConfigJson;
        broker.IsEnabled = req.IsEnabled;

        if (string.IsNullOrWhiteSpace(req.LoginUsername))
        {
            broker.LoginUsername = null;
            broker.LoginPasswordEncrypted = null;
        }
        else
        {
            broker.LoginUsername = req.LoginUsername;
            if (!string.IsNullOrWhiteSpace(req.LoginPassword))
            {
                broker.LoginPasswordEncrypted = credentialProtector.Encrypt(req.LoginPassword);
            }
        }

        var assignmentCount = await db.Assignments.CountAsync(a => a.BrokerId == broker.Id, ct);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendOkAsync(new BrokerResponse(
            broker.Id, broker.Name, broker.WebsiteUrl, broker.AssignmentPortalUrl, broker.LoginUrl, broker.ConnectorKey,
            broker.SelectorConfigJson, broker.IsEnabled, broker.LastScrapedAt, broker.LastScanSucceeded,
            broker.LastScanError, assignmentCount, broker.LoginPasswordEncrypted != null, broker.LoginUsername),
            null, ct);
    }
}
