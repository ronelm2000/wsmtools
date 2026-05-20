namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Put this card to stock and swap bottom card to WR" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードをストック置場に置き、あなたのストックの下から 1 枚を、控え室に置く</c></para>
/// <para><b>Regex:</b> ^このカードをストック置場に置き、あなたのストックの下から 1 枚を、控え室に置く (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Put this card to your stock, and put the bottom card of your stock to your waiting room</c></para>
/// </remarks>
internal class CostPutToStockAndSwapBottomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードをストック置場に置き、あなたのストックの下から1枚を、控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put this card to your stock & Put the bottom card of your stock to your waiting room"
            }
        ];
    }
}
