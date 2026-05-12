namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class FacingCharacterConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラがいないか【リバース】している(?:なら|て)");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is no character facing this card or the character facing this card is [REVERSE]"
            }
        ];
    }
}
