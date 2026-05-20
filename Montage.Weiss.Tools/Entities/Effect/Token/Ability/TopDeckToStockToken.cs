namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class TopDeckToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札の上から(\d+)枚(?:まで)?を、ストック置場に置(?:く|いてよい|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value.Replace("Ｘ", "X");
        var isUpTo = match.Value.Contains("まで");
        var isMay = match.Value.Contains("いてよい");
        var countText = isUpTo ? $"up to {count} card" : $"{count} card";
        if (count != "1") countText += "s";
        var mayText = isMay ? "you may " : "";
        var countPhrase = isUpTo
            ? $"up to {count} card{(count == "1" ? "" : "s")} from the top of your deck"
            : count == "1" ? "the top card of your deck" : $"the top {count} cards of your deck";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"{mayText}put {countPhrase} to your stock"
            }
        ];
    }
}
