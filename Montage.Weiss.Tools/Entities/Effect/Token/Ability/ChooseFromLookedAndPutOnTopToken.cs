namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromLookedAndPutOnTopToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^カードを(\d+)枚(まで)?選び、山札の上に(?:好きな順番で)?置き、残りのカードを控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var isPlural = int.TryParse(count, out var n) && n > 1;
        var cardWord = isPlural ? "cards" : "card";
        var themWord = isPlural ? "them" : "it";
        var fullText = span.ToString();
        var orderText = fullText.Contains("好きな順番で") ? " in any order" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} {cardWord} from among them, put {themWord} on the top of your deck{orderText}, and put the rest to your waiting room"
            }
        ];
    }
}
