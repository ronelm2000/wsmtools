namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class WhenYouUseActConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたが【起】を使った時");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "you use an [ACT]"
            }
        ];
    }
}
