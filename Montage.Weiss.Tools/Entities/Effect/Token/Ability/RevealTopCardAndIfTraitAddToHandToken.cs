namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches reveal-top-card effects with trait-conditional add-to-hand (no discard follow-up).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札の上から1枚を公開する。そのカードが《NIKKE》のキャラなら手札に加える。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?山札の上から1枚を公開する。そのカードが《(.+?)》のキャラなら手札に加える(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>reveal the top card of your deck. If that card is a &lt;&lt;NIKKE&gt;&gt; character, put it to your hand</c></para>
/// </remarks>
internal class RevealTopCardAndIfTraitAddToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?山札の上から1枚を公開する。そのカードが《(.+?)》のキャラなら手札に加える(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["山札の上から1枚を公開する。そのカードが《★TESTTRAIT★》のキャラなら手札に加える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"reveal the top card of your deck. If that card is a <<{trait}>> character, put it to your hand"
            }
        ];
    }
}
