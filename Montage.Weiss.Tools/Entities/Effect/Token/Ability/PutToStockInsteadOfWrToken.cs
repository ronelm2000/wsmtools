namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutToStockInsteadOfWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^控え室に置くかわりにストック置場に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put that card to your stock instead of to your waiting room"
            }
        ];
    }
}
