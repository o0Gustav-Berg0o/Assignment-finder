using System.Net;
using System.Text.Json;
using AssignmentFinder.Domain;
using AssignmentFinder.Scraping;
using AssignmentFinder.Scraping.Connectors;

namespace AssignmentFinder.Tests.Scraping;

public class GenericSelectorConnectorTests
{
    private const string SampleHtml = """
        <html><body>
          <div class="assignment">
            <a class="title" href="/jobs/1">Senior .NET Developer</a>
            <span class="area">Stockholm</span>
            <span class="skills">C#, Azure, SQL</span>
          </div>
          <div class="assignment">
            <a class="title" href="https://broker.example/jobs/2">Java Architect</a>
            <span class="area">Gothenburg</span>
            <span class="skills">Java, Kafka</span>
          </div>
        </body></html>
        """;

    private static readonly CssSelectorConfig Config = new()
    {
        ItemSelector = ".assignment",
        TitleSelector = "a.title",
        LinkSelector = "a.title",
        AreaSelector = "span.area",
        SkillsSelector = "span.skills",
    };

    private static Broker MakeBroker() => new()
    {
        Name = "Test Broker",
        WebsiteUrl = "https://broker.example/",
        AssignmentPortalUrl = "https://broker.example/jobs",
        ConnectorKey = GenericSelectorConnector.ConnectorKey,
        SelectorConfigJson = JsonSerializer.Serialize(Config),
    };

    private static IHttpClientFactory MakeFakeHttpClientFactory(string html)
    {
        var handler = new StubHttpMessageHandler(html);
        var client = new HttpClient(handler);
        return new StubHttpClientFactory(client);
    }

    [Fact]
    public async Task Parses_assignments_from_configured_selectors()
    {
        var connector = new GenericSelectorConnector(MakeFakeHttpClientFactory(SampleHtml));
        var broker = MakeBroker();

        var results = await connector.FetchAsync(broker, decryptedPassword: null, CancellationToken.None);

        Assert.Equal(2, results.Count);

        var first = results[0];
        Assert.Equal("Senior .NET Developer", first.Title);
        Assert.Equal("https://broker.example/jobs/1", first.Url);
        Assert.Equal("Stockholm", first.Area);
        Assert.Equal("C#, Azure, SQL", first.Skills);

        var second = results[1];
        Assert.Equal("https://broker.example/jobs/2", second.Url);
    }

    private sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class StubHttpMessageHandler(string html) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(html) });
    }
}
