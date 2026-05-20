namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentChooseReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?相手の(前列の)?キャラを(\d+)枚(?:まで)?選び、手札に戻す(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isFrontRow = match.Groups[1].Success;
        var count = int.Parse(match.Groups[2].Value);
        var isUpTo = span.ToString().Contains("まで");
        var countText = isUpTo ? $"up to {count}" : count.ToString();
        var locationText = isFrontRow ? " characters in your opponent's center stage" : " of your opponent's characters";
        var pronoun = count == 1 ? "it" : "them";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText}{locationText}"
            },
            new CardEffectAbility
            {
                AbilityText = $"return {pronoun} to their hand"
            }
        ];
    }
}
