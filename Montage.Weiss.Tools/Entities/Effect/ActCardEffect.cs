namespace Montage.Weiss.Tools.Entities.Effect;

public record ActCardEffect : CardEffect, ICostedCardEffect
{
    public override string Type => "Act";
    public required string CostText { get; set; }
    public required List<CardEffectAbility> Cost { get; init; }
}
