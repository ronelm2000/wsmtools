namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ClockTopToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分のクロックの上から(\d+)枚を、?手札に戻す(?:\.|,|、|。)?");
    
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var noun = count == 1 ? "card" : "cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"return the top {count} {noun} of your clock to your hand"
            }
        ];
    }
}
