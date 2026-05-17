namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoFacingCharacterOrReversedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラがいないか【リバース】しているなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "If there is no character facing this card or the character facing this card is [REVERSE]"
            }
        ];
    }
}
