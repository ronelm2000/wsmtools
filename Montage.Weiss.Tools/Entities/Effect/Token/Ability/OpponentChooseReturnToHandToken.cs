namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentChooseReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"相手のキャラを(\d+)枚まで選び、手札に戻す");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} of your opponent's characters, and return it to their hand"
            }
        ];
    }
}
