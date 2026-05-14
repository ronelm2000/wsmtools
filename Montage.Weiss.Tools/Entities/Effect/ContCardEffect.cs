namespace Montage.Weiss.Tools.Entities.Effect;

public record ContCardEffect : CardEffect, IConditionalCardEffect
{
    public override string Type => "Cont";
    public string PreConditionText { get; init; } = string.Empty;
    public string PostConditionText { get; init; } = string.Empty;
    public required string ConditionText { get; init; }
    public required List<CardEffectCondition> Condition { get; init; }
}
