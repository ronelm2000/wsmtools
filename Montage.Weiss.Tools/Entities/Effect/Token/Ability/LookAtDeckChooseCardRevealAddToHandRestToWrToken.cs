namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class LookAtDeckChooseCardRevealAddToHandRestToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の山札を上から(Ｘ|\d+)枚まで見て、(.+?)を(\d+)枚まで選(?:んで相手に見せ|び)、手札に加え、残りのカードを控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var lookCount = match.Groups[1].Value.Replace("Ｘ", "X");
        var cardDesc = match.Groups[2].Value;
        var pickCount = match.Groups[3].Value.Replace("Ｘ", "X");
        var hasReveal = match.Value.Contains("相手に見せ");
        var cardDescEnglish = cardDesc switch
        {
            _ when cardDesc == "カード" => "card",
            _ when cardDesc.Contains("黄のCX") => "yellow CX",
            _ when cardDesc.Contains("CX") => "CX",
            _ when Regex.Match(cardDesc, @"《(.+?)》のキャラ") is Match m && m.Success => $"<<{m.Groups[1].Value}>> character",
            _ when Regex.Match(cardDesc, @"《(.+?)》") is Match m && m.Success => $"<<{m.Groups[1].Value}>>",
            _ => cardDesc
        };
        var revealText = hasReveal ? ", reveal it to your opponent" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"look at up to {lookCount} cards from the top of your deck, choose up to {pickCount} {cardDescEnglish} from among them{revealText}, put it to your hand, and put the rest to your waiting room"
            }
        ];
    }
}
