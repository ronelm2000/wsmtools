namespace Montage.Weiss.Tools.Entities.Effect;

public record ContCardEffect : CardEffect, IConditionalCardEffect
{
    public override string Type => "Cont";
    public required string ConditionText { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }
}
