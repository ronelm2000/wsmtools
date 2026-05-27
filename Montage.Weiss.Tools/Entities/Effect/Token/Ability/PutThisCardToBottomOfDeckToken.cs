namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put this card to the bottom of your deck" cost clauses.
/// Used for act effects that require placing the card under the deck as part of the cost.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードを山札の下に置く</c></para>
/// <para><b>Regex:</b> ^このカードを山札の下に置く(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>put this card to the bottom of your deck</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Alternative verb forms (置いて, 置き)
/// - Full-width characters in 山札 (e.g., 山札の下)</para>
/// </remarks>
internal class PutThisCardToBottomOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを山札の下に置く(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードを山札の下に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put this card to the bottom of your deck"
            }
        ];
    }
}
