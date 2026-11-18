using Microsoft.Extensions.DependencyInjection;

namespace AssignmentFinder.Scraping;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers every IAssignmentConnector implementation in this assembly, plus a shared
    /// named HttpClient ("AssignmentFinder.Scraping") used by HtmlConnectorBase-derived connectors.
    /// Adding a new broker connector class is automatically picked up here - no registration needed.
    /// </summary>
    public static IServiceCollection AddAssignmentConnectors(this IServiceCollection services)
    {
        services.AddHttpClient("AssignmentFinder.Scraping", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "AssignmentFinderBot/1.0 (+personal job-matching tool)");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        var connectorTypes = typeof(ServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IAssignmentConnector).IsAssignableFrom(t));

        foreach (var type in connectorTypes)
        {
            services.AddTransient(typeof(IAssignmentConnector), type);
        }

        return services;
    }
}
