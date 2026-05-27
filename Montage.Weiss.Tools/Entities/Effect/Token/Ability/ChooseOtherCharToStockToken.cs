namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOtherCharToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?他の自分のキャラを1枚選び、ストック置場に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose up to 1 of your other characters, and put it to your stock"
            }
        ];
    }
}
