namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class ExperienceConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^経験\s*あなたのレベル置場に、「.+」と「.+」があるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var c1 = match.Groups["c1"].Value;
        var c2 = match.Groups["c2"].Value;
        // Clean up card names - remove trailing quotes
        c1 = c1.TrimEnd('"');
        c2 = c2.TrimEnd('"');
        // Remove nested double quotes for proper English formatting
        c1 = c1.Replace("\"", "");
        c2 = c2.Replace("\"", "");
        
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"If \"{c1}\" and \"{c2}\" are in your level"
            }
        ];
    }
}
