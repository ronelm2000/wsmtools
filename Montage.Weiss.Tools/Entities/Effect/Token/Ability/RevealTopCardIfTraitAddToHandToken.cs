namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Reveal top card, if trait add to hand" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札の上から 1 枚を公開する。そのカードが《NIKKE》のキャラなら手札に加える。</c></para>
/// <para><b>Regex:</b> ^山札の上から 1 枚を公開する。そのカードが《(.+?)》のキャラなら手札に加える (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Reveal the top card of your deck. If that card is a &lt;&lt;NIKKE&gt;&gt; character, add it to your hand</c></para>
/// </remarks>
internal class RevealTopCardIfTraitAddToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札の上から1枚を公開する。そのカードが《(.+?)》のキャラなら手札に加える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"Reveal the top card of your deck. If that card is a <<{trait}>> character, add it to your hand"
                }
            ];
        }
        return [];
    }
}
