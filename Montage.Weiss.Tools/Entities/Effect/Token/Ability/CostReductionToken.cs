namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostReductionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札のこのカードをプレイするにあたり、あなたは自分の「(.+?)」を1枚選び、控え室に置いてよい。そうしたら、このカードをコスト0でプレイできる。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"""When this card is played in your hand, you may choose 1 of your "{name}", and put it to your waiting room. If you do, you may play this card with 0 cost."""
            }
        ];
    }
}
