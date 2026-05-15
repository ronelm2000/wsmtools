namespace Montage.Weiss.Tools.Entities.Effect;

public record CardEffectCondition
{
    public required ConditionType Type { get; init; }
    public required string ConditionText { get; init; }
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