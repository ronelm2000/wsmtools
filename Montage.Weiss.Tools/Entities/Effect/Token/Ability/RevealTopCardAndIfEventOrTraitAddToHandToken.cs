namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches reveal-top-card effects with event-or-trait conditional add-to-hand + discard chain.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札の上から1枚を公開する。そのカードが《NIKKE》のキャラかイベントなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く。</c></para>
/// <para><b>Regex:</b> ^自分の山札の上から1枚を公開する。そのカードが《(.+?)》のキャラかイベントなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>reveal the top card of your deck. If that card is an event or a &lt;&lt;NIKKE&gt;&gt; character, put it to your hand, choose 1 card in your hand, and put it to your waiting room</c></para>
/// </remarks>
internal class RevealTopCardAndIfEventOrTraitAddToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の山札の上から1枚を公開する。そのカードが《(.+?)》のキャラかイベントなら手札に加え、あなたは自分の手札を1枚選び、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"reveal the top card of your deck. If that card is an event or a <<{trait}>> character, put it to your hand, choose 1 card in your hand, and put it to your waiting room"
            }
        ];
    }
}
