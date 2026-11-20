using AssignmentFinder.Domain;

namespace AssignmentFinder.Api.Shared;

/// <summary>
/// The free-text "query"/"area" search predicate shared by every endpoint that lets a user filter
/// assignments (directly, or via a Match's Assignment navigation) - keeps that matching rule in one
/// place instead of copied per endpoint.
/// </summary>
public static class AssignmentSearchExtensions
{
    public static IQueryable<Assignment> ApplyTextSearch(this IQueryable<Assignment> query, string? textQuery, string? area)
    {
        if (!string.IsNullOrWhiteSpace(textQuery))
        {
            query = query.Where(a =>
                a.Title.Contains(textQuery) ||
                (a.Description != null && a.Description.Contains(textQuery)) ||
                (a.Skills != null && a.Skills.Contains(textQuery)));
        }

        if (!string.IsNullOrWhiteSpace(area))
        {
            query = query.Where(a => a.Area != null && a.Area.Contains(area));
        }

        return query;
    }
}
