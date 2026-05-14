namespace Montage.Weiss.Tools.Entities.Effect;

public interface IConditionalCardEffect
{
    /// <summary>
    /// Pre-conditions occur before `During` conditions, and are separated by periods.
    /// </summary>
    public string PreConditionText { get; init; }
    /// <summary>
    /// Gets the main condition text, which is the combination of "During", "when", and "if" conditions.
    /// </summary>
    public string ConditionText { get; init; }
    /// <summary>
    /// Post-conditions occur after the main effect resolves, and are separated by periods.
    /// </summary>
    public string PostConditionText { get; init; }
    /// <summary>
    /// Gets an ordered list of conditions for the ability to resolve.
    /// </summary>
    public List<CardEffectCondition> Condition { get; init; }
}
