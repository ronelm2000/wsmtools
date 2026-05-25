namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "[REST] this [STAND] card" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>【スタンド】しているこのカードを【レスト】する</c></para>
/// <para><b>Regex:</b> ^【スタンド】しているこのカードを【レスト】する(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>[REST] this card</c></para>
/// </remarks>
internal class CostRestThisCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^【スタンド】しているこのカードを【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["【スタンド】しているこのカードを【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "[REST] this card"
            }
        ];
    }
}
