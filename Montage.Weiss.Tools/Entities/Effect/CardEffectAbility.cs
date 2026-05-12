namespace Montage.Weiss.Tools.Entities.Effect;

public record CardEffectAbility
{
    public required string AbilityText { get; init; }

    public static CardEffectAbility operator +(CardEffectAbility a, CardEffectAbility b)
    {
        return new CardEffectAbility { AbilityText = $"{a.AbilityText}, and {b.AbilityText}" };
    }
}