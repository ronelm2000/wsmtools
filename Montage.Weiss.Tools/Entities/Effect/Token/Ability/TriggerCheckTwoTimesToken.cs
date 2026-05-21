namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Trigger check 2 times" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>トリガーステップにトリガーチェックを 2 回行う。</c></para>
/// <para><b>Regex:</b> ^トリガーステップにトリガーチェックを 2 回行う (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Perform a trigger check 2 times on the trigger step</c></para>
/// </remarks>
internal class TriggerCheckTwoTimesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^トリガーステップにトリガーチェックを2回行う(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Perform a trigger check 2 times on the trigger step"
            }
        ];
    }
}
