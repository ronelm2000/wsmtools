namespace Montage.Weiss.Tools.Entities.Effect;

public record NestedCardEffectAbility : CardEffectAbility
{
    public required CardEffect NestedEffect { get; init; }
}
