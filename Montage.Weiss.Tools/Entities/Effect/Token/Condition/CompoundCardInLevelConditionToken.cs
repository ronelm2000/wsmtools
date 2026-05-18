namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CompoundCardInLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのレベル置場に、「(?<c1>.+?)」と「(?<c2>.+?)」があるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var c1 = match.Groups["c1"].Value;
        var c2 = match.Groups["c2"].Value;

        // Strip curly quotes from card names for cleaner output
        c1 = c1.Replace("\u201C", "").Replace("\u201D", "");
        c2 = c2.Replace("\u201C", "").Replace("\u201D", "");

        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"\"{c1}\" and \"{c2}\" are in your level"
            }
        ];
    }
}
