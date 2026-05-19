namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Matches revealed card type conditions with event-or-trait disjunction.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのカードが《NIKKE》のキャラかイベントなら</c></para>
/// <para><b>Regex:</b> ^そのカードが《(.+?)》のキャラかイベントなら</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>that card is an event or a &lt;&lt;NIKKE&gt;&gt; character</c></para>
/// <para><b>Type:</b> <c>ConditionType.If</c></para>
/// </remarks>
internal class RevealedCardIsEventOrTraitConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^そのカードが《(.+?)》のキャラかイベントなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"that card is an event or a <<{trait}>> character"
            }
        ];
    }
}
