namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "At the beginning of opponent's draw phase" condition clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手のドローフェイズの始めに</c></para>
/// <para><b>Regex:</b> ^相手のドローフェイズの始めに (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>At the beginning of your opponent's draw phase</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c></para>
/// </remarks>
internal class DrawPhaseStartConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^相手のドローフェイズの始めに");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.When,
                ConditionText = "At the beginning of your opponent's draw phase"
            }
        ];
    }
}
