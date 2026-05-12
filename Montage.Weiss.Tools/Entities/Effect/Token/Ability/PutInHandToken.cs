namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutInHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"手札に(?:加え|戻す)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var verb = match.Groups[0].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = verb.Contains("戻す") ? "return it to your hand" : "put it to your hand"
            }
        ];
    }
}
