namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class NoOtherCharacterExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのキャラがいないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there are no other of your characters"
            }
        ];
    }
}
