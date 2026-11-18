namespace AssignmentFinder.Api.Endpoints.Assignments;

public record AssignmentResponse(
    int Id,
    string Title,
    string? Description,
    string? Area,
    string? Skills,
    string Url,
    string BrokerName,
    DateTimeOffset FirstSeenAt,
    bool IsInteresting);

public record AssignmentsPageResponse(List<AssignmentResponse> Items, int TotalCount);
