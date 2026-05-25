namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a [trait] character, rest it, and move to open back row" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他の自分の《NIKKE》のキャラを1枚選び、【レスト】し、後列のキャラのいない枠に動かす。</c></para>
/// <para><b>Regex:</b> ^他の自分の《(.+?)》のキャラを1枚選び、【レスト】し、後列のキャラのいない枠に動かす(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your other &lt;&lt;trait&gt;&gt; characters, [REST] it, and move it to an open position in the back row</c></para>
/// </remarks>
internal class ChooseTraitCharacterRestAndMoveToOpenBackRowToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他の自分の《(.+?)》のキャラを1枚選び、【レスト】し、後列のキャラのいない枠に動かす(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他の自分の《NIKKE》のキャラを1枚選び、【レスト】し、後列のキャラのいない枠に動かす。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 of your other <<{trait}>> characters, [REST] it, and move it to an open position in the back row"
            }
        ];
    }
}
