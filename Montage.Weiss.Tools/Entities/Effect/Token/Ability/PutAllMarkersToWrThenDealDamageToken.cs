namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutAllMarkersToWrThenDealDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのマーカーすべてを、控え室に置き、それらのカードの枚数が(\d+)枚以上なら、相手に1ダメージを(\d+)回与える(?:\.|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードのマーカーすべてを、控え室に置き、それらのカードの枚数が7枚以上なら、相手に1ダメージを12回与える"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (!match.Success)
            return [];
        var threshold = match.Groups[1].Value;
        var times = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put all markers under this card to your waiting room. If the number of cards put were {threshold} or more, deal 1 damage to your opponent {times} times"
            }
        ];
    }
}
