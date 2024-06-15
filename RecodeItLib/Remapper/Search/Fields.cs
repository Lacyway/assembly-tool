﻿using ReCodeIt.Enums;
using ReCodeIt.Models;
using Mono.Cecil;
using MoreLinq;

namespace ReCodeIt.ReMapper.Search;

internal static class Fields
{
    /// <summary>
    /// Returns a match on any type with the provided fields
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parms"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public static EMatchResult IncludeFields(TypeDefinition type, SearchParams parms, ScoringModel score)
    {
        if (parms.IncludeFields is null || parms.IncludeFields.Count == 0) return EMatchResult.Disabled;

        var matches = type.Fields
            .Where(field => parms.IncludeFields.Contains(field.Name));
        score.Score += matches.Count();

        score.FailureReason = matches.Any() ? EFailureReason.None : EFailureReason.FieldsInclude;

        return matches.Any()
            ? EMatchResult.Match
            : EMatchResult.NoMatch;
    }

    /// <summary>
    /// Returns a match on any type without the provided fields
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parms"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public static EMatchResult ExcludeFields(TypeDefinition type, SearchParams parms, ScoringModel score)
    {
        if (parms.ExcludeFields is null || parms.ExcludeFields.Count == 0) return EMatchResult.Disabled;

        var matches = type.Fields
            .Where(field => parms.ExcludeFields.Contains(field.Name))
            .Count();

        score.Score += matches;

        score.FailureReason = matches > 0 ? EFailureReason.None : EFailureReason.FieldsExclude;

        return matches > 0
            ? EMatchResult.NoMatch
            : EMatchResult.Match;
    }

    /// <summary>
    /// Returns a match on any type with a matching number of fields
    /// </summary>
    /// <param name="type"></param>
    /// <param name="parms"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public static EMatchResult MatchFieldCount(TypeDefinition type, SearchParams parms, ScoringModel score)
    {
        if (parms.FieldCount is null) return EMatchResult.Disabled;

        var match = type.Fields.Exactly((int)parms.FieldCount);

        if (match) { score.Score++; }

        score.FailureReason = match ? EFailureReason.None : EFailureReason.FieldsCount;

        return match
            ? EMatchResult.Match
            : EMatchResult.NoMatch;
    }
}