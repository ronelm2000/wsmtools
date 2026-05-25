namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose up to X [trait] characters from waiting room and return to hand" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の控え室の《NIKKE》のキャラをＸ枚まで選び、手札に戻す。</c></para>
/// <para><b>Regex:</b> ^[、,]?(?:あなたは)?(?:自分の)?控え室の《(.+?)》のキャラを[ＸX]枚まで選び、手札に戻す(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose up to X &lt;&lt;NIKKE&gt;&gt; characters in your waiting room, and return them to your hand</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different referents (控え室 vs 控え)
/// - Specified counts instead of X</para>
/// </remarks>
internal class ChooseFromWrTraitXAndReturnToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?控え室の《(.+?)》のキャラを[ＸX]枚まで選び、手札に戻す(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の控え室の《NIKKE》のキャラをＸ枚まで選び、手札に戻す。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to X <<{trait}>> characters in your waiting room, and return them to your hand"
            }
        ];
    }
}
