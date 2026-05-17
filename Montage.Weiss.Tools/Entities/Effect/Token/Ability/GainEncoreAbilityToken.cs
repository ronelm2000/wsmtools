namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Gain Encore ability" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは『【自】 アンコール ［手札の《NIKKE》のキャラを 1 枚控え室に置く］』を得る。</c></para>
/// <para><b>Regex:</b> ^このカードは『\【自\】 アンコール ［手札の《NIKKE》のキャラを 1 枚控え室に置く\］』を得る (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>This card gets "[AUTO] Encore [Put 1 &lt;&lt;NIKKE&gt;&gt; character from your hand to your waiting room]"</c></para>
/// </remarks>
internal class GainEncoreAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは『\【自\】アンコール［手札の《NIKKE》のキャラを1枚控え室に置く\］』を得る");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card gets \"[AUTO] Encore [Put 1 <<NIKKE>> character from your hand to your waiting room]\""
            }
        ];
    }
}
