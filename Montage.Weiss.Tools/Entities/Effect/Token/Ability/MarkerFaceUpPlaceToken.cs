namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MarkerFaceUpPlaceToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの下にマーカーとして表向きに置いてよい(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put it face up under this card as a marker"
            }
        ];
    }
}
