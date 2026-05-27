namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MarkerFaceUpPlaceToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?<thatChar>そのキャラを)?このカードの下にマーカーとして表向きに置いてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードの下にマーカーとして表向きに置いてよい"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var hasThatChar = match.Groups[1].Success;
        return
        [
            new CardEffectAbility
            {
                AbilityText = hasThatChar
                    ? "you may put that character face up under this card as a marker"
                    : "you may put it face up under this card as a marker"
            }
        ];
    }
}
