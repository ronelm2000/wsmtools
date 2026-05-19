namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutThatCharacterToStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのキャラをストック置場に置(?:く|いてよい|き)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isMay = match.Value.Contains("てよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = isMay
                    ? "you may put that character to your opponent's stock"
                    : "put that character to your opponent's stock"
            }
        ];
    }
}
