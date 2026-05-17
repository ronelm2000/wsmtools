namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Exchange level with WR" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>レベル置場のカードと控え室のカードを入れ替える</c></para>
/// <para><b>Regex:</b> ^レベル置場のカードと控え室のカードを入れ替える (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Exchange 1 card in your level with 1 card in your waiting room</c></para>
/// </remarks>
internal class ExchangeLevelWithWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^レベル置場のカードと控え室のカードを入れ替える");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Exchange 1 card in your level with 1 card in your waiting room"
            }
        ];
    }
}
