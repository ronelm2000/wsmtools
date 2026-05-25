namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if there is a level N or lower character among those cards" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>それらのカードにレベル2以下のキャラがあるなら</c></para>
/// <para><b>Regex:</b> ^それらのカードにレベル(\d+)以下のキャラがあるなら(?:、|\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level threshold</description></item>
/// </list>
/// <para><b>Output:</b> <c>If there is a level {N} or lower character among those cards</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class IfLowerLevelCharacterAmongThoseCardsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^それらのカードにレベル(\d+)以下のキャラがあるなら(?:、|\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["それらのカードにレベル2以下のキャラがあるなら"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there is a level {level} or lower character among those cards"
            }
        ];
    }
}
