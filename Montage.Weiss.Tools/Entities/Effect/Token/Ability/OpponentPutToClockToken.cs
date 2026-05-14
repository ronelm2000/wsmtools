namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentPutToClockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはそのキャラをクロック置場に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put that character to your opponent's clock"
            }
        ];
    }
}
