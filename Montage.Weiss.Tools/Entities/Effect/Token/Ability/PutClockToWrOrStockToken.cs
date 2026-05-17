namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Put clock top card to WR or stock" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>クロックの上から 1 枚を、控え室に置く。...控え室に置くかわりにストック置場に置いてよい。</c></para>
/// <para><b>Regex:</b> ^クロックの上から 1 枚を、控え室に置く (?:。|\.)?(?:そのカードを|控え室に置くかわりに) ストック置場に置いてよい (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Put the top card of your clock to your waiting room. You may put that card to your stock instead of putting it to your waiting room</c></para>
/// </remarks>
internal class PutClockToWrOrStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^クロックの上から1枚を、控え室に置く(?:。|\.)?(?:そのカードを|控え室に置くかわりに)ストック置場に置いてよい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put the top card of your clock to your waiting room. You may put that card to your stock instead of putting it to your waiting room"
            }
        ];
    }
}
