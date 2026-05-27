namespace Montage.Weiss.Tools.Entities.Effect;

public record EventCardEffect : CardEffect, ICostedCardEffect, IConditionalCardEffect
{
    public override string Type => "Event";
    public string CostText { get; set; } = string.Empty;
    public List<CardEffectAbility> Cost { get; init; } = [];
    public string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public string ConditionText { get; init; } = string.Empty;
    public List<CardEffectCondition> Condition { get; init; } = [];
}
