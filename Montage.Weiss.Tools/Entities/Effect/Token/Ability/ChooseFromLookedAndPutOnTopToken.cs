namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromLookedAndPutOnTopToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^カードを(\d+)枚(まで)?選び、山札の上に置き、残りのカードを控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} card from among them, put it on the top of your deck, and put the rest to your waiting room"
            }
        ];
    }
}
