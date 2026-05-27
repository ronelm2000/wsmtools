namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class YourCxTriggeredConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのCXがトリガーした時");

    public override IEnumerable<string> SampleMatches => ["あなたのCXがトリガーした時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "your CX triggers"
            }
        ];
    }
}
