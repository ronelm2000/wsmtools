namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches "damage you received was not canceled" condition clauses, with optional during-battle prefix.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのバトル中、あなたの受けたダメージがキャンセルされなかった時</c></para>
/// <para><b>Regex:</b> ^(このカードのバトル中、)?あなたの受けたダメージがキャンセルされなかった時</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Optional "このカードのバトル中、" prefix</description></item>
/// </list>
/// <para><b>Output:</b> <c>During this card's battle, when damage you received was not canceled</c></para>
/// <para><b>Type:</b> <c>ConditionType.When</c> (plus ConditionType.During if prefix present)</para>
/// </remarks>
internal class DamageYouReceivedNotCanceledConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^(このカードのバトル中、)?あなたの受けたダメージがキャンセルされなかった時");
    public override IEnumerable<string> SampleMatches => ["このカードのバトル中、あなたの受けたダメージがキャンセルされなかった時"];

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var duringBattle = match.Groups[1].Success;
        var conditions = new List<CardEffectCondition>();
        if (duringBattle)
        {
            conditions.Add(new CardEffectCondition
            {
                Type = ConditionType.During,
                ConditionText = "this card's battle"
            });
        }
        conditions.Add(new CardEffectCondition
        {
            Type = ConditionType.When,
            ConditionText = "damage you received was not canceled"
        });
        return conditions;
    }
}
