namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class EitherPlayerTopCardsToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分|相手|自分か相手)の、山札の上から(\d+)枚を、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var input = span.ToString();
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"put the top {count} cards of your or your opponent's deck to your waiting room"
            }
        ];
    }
}
