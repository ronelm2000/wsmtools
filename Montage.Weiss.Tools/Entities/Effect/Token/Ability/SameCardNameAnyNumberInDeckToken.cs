namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "cards with same card name can be in deck any number" continuous effect clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードと同じカード名のカードは、デッキに好きな枚数入れることができる。</c></para>
/// <para><b>Regex:</b> ^このカードと同じカード名のカードは、デッキに好きな枚数入れることができる(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>You can put any number of cards with the same card name as this card in your deck</c></para>
/// <para><b>Usage:</b> This is a static-output token — the entire clause is fixed wording (no capture groups).</para>
/// </remarks>
internal class SameCardNameAnyNumberInDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードと同じカード名のカードは、デッキに好きな枚数入れることができる(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["このカードと同じカード名のカードは、デッキに好きな枚数入れることができる。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "You can put any number of cards with the same card name as this card in your deck"
            }
        ];
    }
}
