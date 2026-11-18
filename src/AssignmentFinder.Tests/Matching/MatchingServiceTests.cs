using AssignmentFinder.Domain;
using AssignmentFinder.Infrastructure.Matching;

namespace AssignmentFinder.Tests.Matching;

public class MatchingServiceTests
{
    private readonly MatchingService sut = new();

    private static Assignment MakeAssignment(string area, string title, string? skills = null) => new()
    {
        ExternalId = "1",
        Title = title,
        Url = "https://example.com/1",
        Area = area,
        Skills = skills,
    };

    private static SearchCriterion MakeCriterion(
        string area, string requirements, int minMatches = 1) => new()
    {
        UserId = "user-1",
        Name = "test",
        AreaKeywords = area,
        RequirementKeywords = requirements,
        MinRequirementMatches = minMatches,
    };

    [Fact]
    public void Matches_when_area_and_requirement_both_hit()
    {
        var assignment = MakeAssignment("Stockholm", "Senior .NET developer", "C#,Azure");
        var criterion = MakeCriterion("Stockholm,Remote", "C#,Java");

        Assert.True(sut.IsMatch(assignment, criterion));
    }

    [Fact]
    public void Does_not_match_when_area_misses()
    {
        var assignment = MakeAssignment("Gothenburg", "Senior .NET developer", "C#");
        var criterion = MakeCriterion("Stockholm", "C#");

        Assert.False(sut.IsMatch(assignment, criterion));
    }

    [Fact]
    public void Does_not_match_when_requirement_count_below_minimum()
    {
        var assignment = MakeAssignment("Stockholm", "Senior .NET developer", "C#");
        var criterion = MakeCriterion("Stockholm", "C#,Azure,Kubernetes", minMatches: 2);

        Assert.False(sut.IsMatch(assignment, criterion));
    }

    [Fact]
    public void Matches_when_requirement_count_meets_minimum()
    {
        var assignment = MakeAssignment("Stockholm", "Senior .NET developer", "C#,Azure");
        var criterion = MakeCriterion("Stockholm", "C#,Azure,Kubernetes", minMatches: 2);

        Assert.True(sut.IsMatch(assignment, criterion));
    }

    [Fact]
    public void Empty_area_keywords_means_any_area_matches()
    {
        var assignment = MakeAssignment("Gothenburg", "Senior .NET developer", "C#");
        var criterion = MakeCriterion(area: "", requirements: "C#");

        Assert.True(sut.IsMatch(assignment, criterion));
    }

    [Fact]
    public void Is_case_insensitive()
    {
        var assignment = MakeAssignment("stockholm", "developer", "c#");
        var criterion = MakeCriterion("STOCKHOLM", "C#");

        Assert.True(sut.IsMatch(assignment, criterion));
    }
}
