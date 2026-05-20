using Montage.Card.API.Utilities;

namespace Montage.Weiss.Tools.Entities.Effect;

public enum ConditionConjunction
{
    /// <summary>Default. Conditions joined by "and": "if X, and Y, and Z".</summary>
    And,
    /// <summary>`か` / `または` — "or". Output: "if X or Y".</summary>
    Or,
    /// <summary>`で` — continuative "and" for conditions. Same output as And but semantically distinct.</summary>
    Continuation,
}

public record CardEffectCondition
{
    public required ConditionType Type { get; init; }
    public required string ConditionText { get; init; }
    public ConditionConjunction Conjunction { get; init; } = ConditionConjunction.And;
    public bool IsUnmatched { get; init; } = false;
}

public static class CardEffectConditionExtensions {
    /// <summary>
    /// Aggregates a list of <see cref="CardEffectCondition"/> into a single English condition string.
    /// </summary>
    /// <remarks>
    /// Groups conditions by <see cref="ConditionType"/> (During, When, If, At), then joins them
    /// with appropriate conjunctions based on <see cref="ConditionConjunction"/>. Pre-conditions
    /// and post-conditions are skipped as they are anchored and inserted by the parent CardEffect.
    /// </remarks>
    /// <param name="conditions">The conditions to aggregate.</param>
    /// <returns>A formatted English condition string, or empty string if no conditions.</returns>
    /// <example>
    /// Input: [During your turn], [if you have 3 or more other characters]
    /// Output: "During your turn, if you have 3 or more other characters"
    /// </example>
    public static string AggregateToString(this IEnumerable<CardEffectCondition> conditions)
    {
        if (!conditions.Any()) return string.Empty;
        var typeGroups = conditions.GroupBy(c => c.Type).ToList();
        var parts = new List<string>();
        bool isFirst = true;

        foreach (var typeGroup in typeGroups)
        {
            var typeStr = typeGroup.Key switch
            {
                ConditionType.When => "When",
                ConditionType.If => "If",
                ConditionType.During => "During",
                ConditionType.At => "At",
                ConditionType.For => "For each",
                ConditionType.PreCondition => "",
                ConditionType.PostCondition => "",
                _ => throw new InvalidOperationException("Unknown condition type")
            };
            if (typeGroup.Key == ConditionType.PreCondition || typeGroup.Key == ConditionType.PostCondition)
            {
                continue;
            }

            var conjunctionGroups = typeGroup.GroupBy(c => c.Conjunction).ToList();
            var conjunctionParts = new List<string>();
            foreach (var conjunctionGroup in conjunctionGroups)
            {
                var conjunctionStr = conjunctionGroup.Key switch
                {
                    ConditionConjunction.And => "and",
                    ConditionConjunction.Or => "or",
                    ConditionConjunction.Continuation => "and",
                    _ => throw new InvalidOperationException("Unknown conjunction")
                };
                conjunctionParts.Add(conjunctionGroup.Select(c => c.ConditionText).JoinWithOxfordComma(conjunctionStr, true));
            }
            var combinedConjunctions = conjunctionParts.JoinWithOxfordComma();
            var part = $"{typeStr} {combinedConjunctions}".Trim();
            if (!isFirst)
            {
                part = char.ToLower(part[0]) + part[1..];
            }
            parts.Add(part);
            isFirst = false;
        }
        return string.Join(", ", parts);
    }
}

public enum ConditionType
{
    /// <summary>
    /// When conditions are activation conditions, and usually are only reserved for [AUTO] abilities. These should only occur after "During" conditions.
    /// </summary>
    When,
    /// <summary>
    /// If conditions are the default condition, and can occur after "When" conditions. These can also occur between actions if additional conditions are involved.
    /// </summary>
    If,
    /// <summary>
    /// During conditions are dependent on turn progression, such as "During your turn", or "During your opponent's turn".
    /// </summary>
    During,
    /// <summary>
    /// At conditions are dependent on a specific timing, such as "At the end of the turn", or "At the beginning of your main phase". These should only occur after "During" conditions.
    /// </summary>
    At,
    /// <summary>
    /// For conditions indicate a "per X" or "for each X" relationship, e.g. "marker underneath this card".
    /// The prefix "For each" is prepended by <see cref="CardEffectConditionExtensions.AggregateToString"/>
    /// following the standard ConditionType pattern (unlike PreCondition/PostCondition which are anchored externally).
    /// </summary>
    For,
    /// <summary>
    /// Pre-conditions occur before `During` conditions, and are separated by JP periods.
    /// "This ability activates up to 1 time per turn" for example, is a pre-condition.
    /// </summary>
    PreCondition,
    /// <summary>
    /// Post-conditions occur after the main effect resolves, and are separated by JP periods.
    /// "X is equal to" for example, should be a post-condition.
    /// </summary>
    PostCondition
}