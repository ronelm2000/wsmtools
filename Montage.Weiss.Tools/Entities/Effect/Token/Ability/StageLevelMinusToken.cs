namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "this card on stage gets -level" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>舞台のこのカードのレベルを－1。</c></para>
/// <para><b>Regex:</b> ^舞台のこのカードのレベルを－(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level reduction value</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card on stage gets -{level} level</c></para>
/// </remarks>
internal class StageLevelMinusToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^舞台のこのカードのレベルを－(\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["舞台のこのカードのレベルを－1。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card on stage gets -{level} level"
            }
        ];
    }
}
