namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches search-deck-for-CX + exchange-with-hand CX clauses.
/// Emits atomic abilities — each returned <see cref="CardEffectAbility"/> represents a single action.
/// The parent (e.g. <see cref="AutoEffectToken"/> via <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/>)
/// joins them with appropriate connectors (", " then ", and ").
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の山札を見てCXを1枚まで選んで公開し、自分の手札のCXを1枚まで選んで公開し、それらのCXを入れ替え、その山札をシャッフルする。</c></para>
/// <para><b>Regex:</b> ^あなたは自分の山札を見てCXを1枚まで選んで公開し、自分の手札のCXを1枚まで選んで公開し、それらのCXを入れ替え、その山札をシャッフルする(?:\.|,|、|。)?</para>
/// <para><b>Output (6 atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description>0: <c>search your deck for up to 1 CX</c></description></item>
///   <item><description>1: <c>reveal it to your opponent</c></description></item>
///   <item><description>2: <c>choose up to 1 CX in your hand</c></description></item>
///   <item><description>3: <c>reveal it to your opponent</c></description></item>
///   <item><description>4: <c>exchange them</c></description></item>
///   <item><description>5: <c>shuffle your deck</c></description></item>
/// </list>
/// <para><b>Atomic Ability Pattern:</b> See <c>SearchDeckLevelAndCostToken</c> for the rationale;
/// this token follows the same decomposition pattern.</para>
/// </remarks>
internal class SearchDeckForCxAndExchangeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見てCXを1枚まで選んで公開し、自分の手札のCXを1枚まで選んで公開し、それらのCXを入れ替え、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility { AbilityText = "search your deck for up to 1 CX" },
            new CardEffectAbility { AbilityText = "reveal it to your opponent" },
            new CardEffectAbility { AbilityText = "choose up to 1 CX in your hand" },
            new CardEffectAbility { AbilityText = "reveal it to your opponent" },
            new CardEffectAbility { AbilityText = "exchange them" },
            new CardEffectAbility { AbilityText = "shuffle your deck" }
        ];
    }
}
