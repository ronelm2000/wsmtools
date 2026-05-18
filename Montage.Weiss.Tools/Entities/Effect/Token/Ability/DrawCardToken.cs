namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DrawCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは(\d+)枚引く(?:\.|,|、|。)?");
    
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var noun = count == 1 ? "card" : "cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"draw {count} {noun}"
            }
        ];
    }
}
