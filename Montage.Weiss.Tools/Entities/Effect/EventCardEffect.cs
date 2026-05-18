namespace Montage.Weiss.Tools.Entities.Effect;

public record EventCardEffect : CardEffect, ICostedCardEffect
{
    public override string Type => "Event";
    public string CostText { get; set; } = string.Empty;
    public List<CardEffectAbility> Cost { get; init; } = [];
}
