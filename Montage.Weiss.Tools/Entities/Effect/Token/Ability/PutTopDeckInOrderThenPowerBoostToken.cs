namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutTopDeckInOrderThenPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札の上に好きな順番で置き、そのターン中、このカードのパワーを[＋\+](\d+)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put them on top of your deck in any order, and this card gets +{power} power until end of turn"
            }
        ];
    }
}
