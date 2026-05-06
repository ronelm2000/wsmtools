namespace Montage.Weiss.Tools.Entities.Effect;

public interface ICostedCardEffect
{
    public string CostText { get; set; }
    public List<CardEffectAbility> Cost { get; init; }
}
