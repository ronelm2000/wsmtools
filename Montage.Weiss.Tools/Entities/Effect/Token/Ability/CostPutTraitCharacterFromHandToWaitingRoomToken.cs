namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches encore cost "put 1 trait character from hand to waiting room" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>手札の《★TESTTRAIT★》のキャラを1枚控え室に置く。</c></para>
/// <para><b>Regex:</b> ^手札の《(.+?)》のキャラを1枚控え室に置(?:く|き)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "サマポケ")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Put 1 &lt;&lt;{trait}&gt;&gt; character from your hand to your waiting room</c></para>
/// </remarks>
internal class CostPutTraitCharacterFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札の《(.+?)》のキャラを1枚控え室に置(?:く|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["手札の《★TESTTRAIT★》のキャラを1枚控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 <<{trait}>> character from your hand to your waiting room"
            }
        ];
    }
}
