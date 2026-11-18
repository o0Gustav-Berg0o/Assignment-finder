namespace AssignmentFinder.Scraping;

/// <summary>
/// Describes, in plain CSS selectors, where to find assignment data on a broker's listing page.
/// Stored as JSON on Broker.SelectorConfigJson. Selectors are resolved relative to ItemSelector
/// (so TitleSelector etc. should be scoped to a single assignment card/row), except ItemSelector
/// itself which is resolved against the whole document.
/// </summary>
public record CssSelectorConfig
{
    /// <summary>Selector for the repeating element representing one assignment (card/row/li).</summary>
    public required string ItemSelector { get; init; }

    /// <summary>Selector (within an item) for the title text.</summary>
    public required string TitleSelector { get; init; }

    /// <summary>Selector (within an item) for the anchor linking to the assignment's detail page.</summary>
    public required string LinkSelector { get; init; }

    /// <summary>Optional selector (within an item) for the area/location text.</summary>
    public string? AreaSelector { get; init; }

    /// <summary>Optional selector (within an item) for a skills/requirements summary text.</summary>
    public string? SkillsSelector { get; init; }

    /// <summary>Optional selector (within an item) for a short description/summary text.</summary>
    public string? DescriptionSelector { get; init; }
}
