using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Data;
using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Scanning;
using FastEndpoints;
using FluentValidation;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public class UpsertBrokerRequestValidator : Validator<UpsertBrokerRequest>
{
    public UpsertBrokerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.WebsiteUrl).NotEmpty();
        RuleFor(x => x.ConnectorKey).NotEmpty();
        RuleFor(x => x.LoginPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.LoginUsername))
            .WithMessage("LoginPassword is required when LoginUsername is set.");
    }
}

public class CreateBrokerEndpoint(AppDbContext db, BrokerCredentialProtector credentialProtector)
    : Endpoint<UpsertBrokerRequest, BrokerResponse>
{
    public override void Configure()
    {
        Post("/api/brokers");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(UpsertBrokerRequest req, CancellationToken ct)
    {
        var broker = new Broker
        {
            Name = req.Name,
            WebsiteUrl = req.WebsiteUrl,
            AssignmentPortalUrl = req.AssignmentPortalUrl,
            LoginUrl = req.LoginUrl,
            ConnectorKey = req.ConnectorKey,
            SelectorConfigJson = req.SelectorConfigJson,
            IsEnabled = req.IsEnabled,
            LoginUsername = string.IsNullOrWhiteSpace(req.LoginUsername) ? null : req.LoginUsername,
            LoginPasswordEncrypted = string.IsNullOrWhiteSpace(req.LoginPassword)
                ? null
                : credentialProtector.Encrypt(req.LoginPassword),
        };

        db.Brokers.Add(broker);
        await db.SaveChangesAsync(ct);

        await HttpContext.Response.SendOkAsync(new BrokerResponse(
            broker.Id, broker.Name, broker.WebsiteUrl, broker.AssignmentPortalUrl, broker.LoginUrl, broker.ConnectorKey,
            broker.SelectorConfigJson, broker.IsEnabled, broker.LastScrapedAt, broker.LastScanSucceeded,
            broker.LastScanError, 0, broker.LoginPasswordEncrypted != null, broker.LoginUsername), null, ct);
    }
}
