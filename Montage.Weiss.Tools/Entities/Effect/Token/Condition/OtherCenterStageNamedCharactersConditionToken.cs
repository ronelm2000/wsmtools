namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherCenterStageNamedCharactersConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたの前列の、「(.+?)」と「(.+?)」がいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name1 = Regex.Replace(match.Groups[1].Value, @"[\u201c\u201d\u0022]", "");
        var name2 = Regex.Replace(match.Groups[2].Value, @"[\u201c\u201d\u0022]", "");
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you have another {name1} and {name2} in your center stage",
                Conjunction = ConditionConjunction.And
            }
        ];
    }
}
