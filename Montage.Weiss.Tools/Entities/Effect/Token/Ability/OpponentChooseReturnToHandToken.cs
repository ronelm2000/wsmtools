namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentChooseReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?相手の(前列の)?キャラを(\d+)枚まで選び、手札に戻す(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isFrontRow = match.Groups[1].Success;
        var count = int.Parse(match.Groups[2].Value);
        var locationText = isFrontRow ? " characters in your opponent's center stage" : " of your opponent's characters";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count}{locationText}, and return them to your opponent's hand"
            }
        ];
    }
}
