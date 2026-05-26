namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ClockSwapToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分のクロックを(\d+)枚選び、手札に戻してよい。そうしたら、あなたは自分の手札を(\d+)枚選び、クロック置場に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var takeCount = match.Groups[1].Value;
        var putCount = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may put {takeCount} card from your clock into your hand. If you do, choose {putCount} card from your hand and put it into your clock"
            }
        ];
    }
}
