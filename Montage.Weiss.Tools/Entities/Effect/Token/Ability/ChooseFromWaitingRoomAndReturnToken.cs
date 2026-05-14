namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の控え室の(《(.+?)》の)?キャラを(\d+)枚選び、(?:手札に戻す|このカードの下にマーカーとして表向きに置いてよい)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[2].Success ? match.Groups[2].Value : null;
        var count = int.Parse(match.Groups[3].Value);
        var traitText = trait != null ? $" <<{trait}>>" : "";
        var action = match.Groups[4].Success ? match.Groups[4].Value : string.Empty;
        
        if (action.Contains("マーカーとして", StringComparison.Ordinal))
        {
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"choose {count}{traitText} character in your waiting room, and put it face up underneath this card as a marker"
                }
            ];
        }
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count}{traitText} character in your waiting room, and return it to your hand"
            }
        ];
    }
}
