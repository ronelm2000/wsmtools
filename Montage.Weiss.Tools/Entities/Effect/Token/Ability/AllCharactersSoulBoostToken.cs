namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllCharactersSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのキャラすべてに、ソウルを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var soul = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your characters get +{soul} soul"
            }
        ];
    }
}
