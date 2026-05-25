namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "Card exists in memory" condition clauses.
/// Uses <see cref="ConditionType.LocationIf"/> so location-based If conditions are grouped separately
/// from standard If conditions, ensuring correct ordering in <see cref="CardEffectConditionExtensions.AggregateToString"/>
/// (e.g. "If this card is in your memory, when ..., if your level is ...").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>思い出置場にこのカードがあり</c></para>
/// <para><b>Regex:</b> ^思い出置場にこのカードがあり</para>
/// <para><b>Output:</b> <c>If this card is in your memory</c></para>
/// <para><b>Type:</b> <c>ConditionType.LocationIf</c></para>
/// </remarks>
internal class CardExistsInMemoryConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^思い出置場にこのカードがあ(?:り|る)(?:ます)?(?:なら)?");
    public override IEnumerable<string> SampleMatches => ["思い出置場にこのカードがあり"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.LocationIf,
                ConditionText = "this card is in your memory"
            }
        ];
    }
}
