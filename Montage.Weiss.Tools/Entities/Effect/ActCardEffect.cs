namespace Montage.Weiss.Tools.Entities.Effect;

public record ActCardEffect : CardEffect, ICostedCardEffect, IConditionalCardEffect
{
    public override string Type => "Act";
    public required string CostText { get; set; }
    public required List<CardEffectAbility> Cost { get; init; }
    public string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public string ConditionText { get; init; } = string.Empty;
    public List<CardEffectCondition> Condition { get; init; } = [];
}
