namespace Montage.Weiss.Tools.Entities.Effect;

public record CardEffectCondition
{
    public required ConditionType Type { get; init; }
    public required string ConditionText { get; init; }
}

public enum ConditionType
{
    When,
    If,
    During
}