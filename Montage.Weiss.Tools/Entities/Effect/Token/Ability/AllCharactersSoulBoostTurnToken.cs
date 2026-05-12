namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllCharactersSoulBoostTurnToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのキャラすべてに、そのターン中、ソウルを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var soul = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your characters get +{soul} soul until end of turn"
            }
        ];
    }
}
