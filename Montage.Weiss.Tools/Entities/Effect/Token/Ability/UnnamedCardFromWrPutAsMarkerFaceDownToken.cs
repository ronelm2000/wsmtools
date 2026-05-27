namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class UnnamedCardFromWrPutAsMarkerFaceDownToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^控え室のカードを1枚選び、このカードの下にマーカーとして裏向きに置く(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 card in your waiting room, and put it face down under this card as a marker"
            }
        ];
    }
}
