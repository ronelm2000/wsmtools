namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "Card exists in memory" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>思い出置場にこのカードがあり</c></para>
/// <para><b>Regex:</b> ^思い出置場にこのカードがあり (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>If this card is in your memory</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class CardExistsInMemoryConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^思い出置場にこのカードがあり");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "If this card is in your memory"
            }
        ];
    }
}
