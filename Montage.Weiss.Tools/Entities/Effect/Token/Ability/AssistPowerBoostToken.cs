namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AssistPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカードの前のあなたのキャラすべてに、パワーを＋Ｘ。Ｘはそのキャラのレベル×(\d+)に等しい。");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var multiplier = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your characters in front of this card get +X power. X is equal to that character's level x{multiplier}"
            }
        ];
    }
}
