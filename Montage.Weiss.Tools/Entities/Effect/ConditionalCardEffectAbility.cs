namespace Montage.Weiss.Tools.Entities.Effect;

/// <summary>
/// A card effect ability that is conditional on certain conditions. This is used for effects that have additional conditions like "If you revealed a CX, you may...".
/// </summary>
public record ConditionalCardEffectAbility : CardEffectAbility, IConditionalCardEffect, ICardAbility
{
    public string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public string ActualAbilityText { get; init; } = string.Empty;
    public required string ConditionText { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }
    public required override string AbilityText
    {
        get => $"{Condition.AggregateToString()}, {ActualAbilityText}".Trim();
        init => ActualAbilityText = value;
    }
}
