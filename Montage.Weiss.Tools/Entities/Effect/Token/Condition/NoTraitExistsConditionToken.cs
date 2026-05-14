namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(?:このカードは、)?あなた(?:に|の)《(.+?)》のキャラがいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.If,ConditionText = $"If you do not have a <<{trait}>> character"
            }
        ];
    }
}
