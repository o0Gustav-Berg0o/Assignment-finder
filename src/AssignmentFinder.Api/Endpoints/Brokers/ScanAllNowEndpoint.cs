using AssignmentFinder.Infrastructure.Identity;
using AssignmentFinder.Infrastructure.Scanning;
using FastEndpoints;

namespace AssignmentFinder.Api.Endpoints.Brokers;

public record ScanAllNowResponse(int BrokersScanned, int NewAssignments, int NewMatches);

public class ScanAllNowEndpoint(ScanPipeline pipeline) : EndpointWithoutRequest<ScanAllNowResponse>
{
    public override void Configure()
    {
        Post("/api/brokers/scan-all");
        Roles(ApplicationUser.AdminRole);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await pipeline.RunAsync(ct);

        await HttpContext.Response.SendOkAsync(
            new ScanAllNowResponse(result.BrokersScanned, result.NewAssignments, result.NewMatches), null, ct);
    }
}
