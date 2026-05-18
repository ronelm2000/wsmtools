namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "No color cards in level" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベル置場に色カードがないなら</c></para>
/// <para><b>Regex:</b> ^レベル置場に色カードがないなら (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>If there are no color cards in your level</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class NoColorCardsInLevelConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^レベル置場に色カードがないなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there are no color cards in your level"
            }
        ];
    }
}
