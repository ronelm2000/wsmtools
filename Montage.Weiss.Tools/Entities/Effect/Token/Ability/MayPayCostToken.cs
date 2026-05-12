namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MayPayCostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはコストを払ってよい。そうたら、");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may pay the cost. If you do"
            }
        ];
    }
}
