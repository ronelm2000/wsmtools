namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PlaceOnOpenPositionAndDealDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを舞台のキャラのいない枠に置き、相手に(\d+)ダメージを(\d+)回与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var damage = match.Groups[1].Value;
        var times = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put this card in an open position of your stage"
            },
            new CardEffectAbility
            {
                AbilityText = $"deal {damage} damage to your opponent {times} times"
            }
        ];
    }
}
