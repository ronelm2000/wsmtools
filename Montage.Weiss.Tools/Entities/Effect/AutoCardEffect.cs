namespace Montage.Weiss.Tools.Entities.Effect;

public record AutoCardEffect : CardEffect, ICostedCardEffect, IConditionalCardEffect
{
    public override string Type => "Auto";
    public required string ConditionText { get; init; }
    public required string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public string CostText { get; set; } = string.Empty;
    public required List<CardEffectAbility> Cost { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }


}
