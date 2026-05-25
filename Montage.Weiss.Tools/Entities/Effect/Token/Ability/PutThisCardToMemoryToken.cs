namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put this card to memory" clauses, with optional "you may" variants.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードを思い出にする</c> or <c>このカードを思い出してもよい</c></para>
/// <para><b>Regex:</b> ^このカードを思い出に(?:してもよい|してよい|する)(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>put this card to your memory</c> or <c>you may put this card to your memory</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Additional optional variants</para>
/// </remarks>
internal class PutThisCardToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを思い出に(?:してもよい|してよい|する)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードを思い出にする。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var fullText = span.ToString();
        var isOptional = fullText.Contains("してもよい") || fullText.Contains("してよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = isOptional ? "you may put this card to your memory" : "put this card to your memory"
            }
        ];
    }
}
