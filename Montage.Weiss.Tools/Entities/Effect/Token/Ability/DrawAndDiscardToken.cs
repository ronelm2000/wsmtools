namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DrawAndDiscardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは(\d+)枚引いてよい。そうしたら、あなたは自分の手札を(\d+)枚選び、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var drawCount = int.Parse(match.Groups[1].Value);
        var discardCount = int.Parse(match.Groups[2].Value);
        var pronoun = discardCount == 1 ? "it" : "them";
        var discardNoun = discardCount == 1 ? "card" : "cards";
        var drawNoun = drawCount == 1 ? "card" : "cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may draw {drawCount} {drawNoun}. If you do, choose {discardCount} {discardNoun} in your hand, and put {pronoun} to your waiting room"
            }
        ];
    }
}
