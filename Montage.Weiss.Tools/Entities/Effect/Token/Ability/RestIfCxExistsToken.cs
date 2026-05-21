namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST this card if CX exists" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>それらのカードに CX があるなら、このカードを【レスト】する。</c></para>
/// <para><b>Regex:</b> ^それらのカードに CX があるなら、このカードを【レスト】する (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>If there is a CX among those cards, [REST] this card</c></para>
/// </remarks>
internal class RestIfCxExistsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^それらのカードにCXがあるなら、このカードを【レスト】する(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "If there is a CX among those cards, [REST] this card"
            }
        ];
    }
}
