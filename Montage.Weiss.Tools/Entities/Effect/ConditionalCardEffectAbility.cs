namespace Montage.Weiss.Tools.Entities.Effect;

/// <summary>
/// A card effect ability that is conditional on certain conditions. This is used for effects that have additional conditions like "If you revealed a CX, you may...".
/// </summary>
public record ConditionalCardEffectAbility : IConditionalCardEffect
{
    public string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public string ConditionText { get; init; }
    public List<CardEffectCondition> Condition { get; init; }
    public required string AbilityText { get; init; }
}
