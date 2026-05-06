namespace Montage.Weiss.Tools.Entities.Effect;

public interface IConditionalCardEffect
{
    public string ConditionText { get; init; }
    public List<CardEffectCondition> Condition { get; init; }
}
