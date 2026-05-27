namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "if there is a CX among the revealed cards" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>この効果で公開されたカードにCXがあるなら</c></para>
/// <para><b>Regex:</b> ^この効果で公開されたカードにCXがあるなら</para>
/// <para><b>Output:</b> <c>there is a CX among the revealed cards</c> (as If-type condition)</para>
/// <para><b>Usage:</b> Follows reveal effects where the player may gain an extra benefit if a CX was revealed.</para>
/// </remarks>
internal class RevealedCardsContainCxConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^この効果で公開されたカードにCXがあるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = "there is a CX among the revealed cards"
            }
        ];
    }
}
