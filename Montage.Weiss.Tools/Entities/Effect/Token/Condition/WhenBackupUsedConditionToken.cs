namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class WhenBackupUsedConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたがこのカードの『助太刀』を使った時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "you use this card's \"Backup\""
            }
        ];
    }
}
