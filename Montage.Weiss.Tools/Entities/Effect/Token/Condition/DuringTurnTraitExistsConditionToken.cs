namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnTraitExistsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのターン中、他のあなたの《.+》のキャラがいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups["trait"].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = $"During your turn, if you have another <<{trait}>> character"
            }
        ];
    }
}
