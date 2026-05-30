namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DrawOrBounceAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(\d+)枚まで引くか相手のキャラを(\d+)枚まで選び、手札に戻す(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var drawCount = int.Parse(match.Groups[1].Value);
        var bounceCount = int.Parse(match.Groups[2].Value);
        var drawNoun = drawCount == 1 ? "card" : "cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"either draw up to {drawCount} {drawNoun}, or choose up to {bounceCount} of your opponent's characters, and return it to your hand"
            }
        ];
    }
}
