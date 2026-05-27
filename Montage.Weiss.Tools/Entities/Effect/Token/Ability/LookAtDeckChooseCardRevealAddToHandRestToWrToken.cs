namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "look at top N cards of deck, choose up to M [trait] cards from among them, reveal, put to hand, put rest to WR" chains.
/// Emits five atomic abilities for proper conjunction handling by the parent token.
/// Supports level-based card descriptions (e.g., "レベル1以下のキャラ", "レベル1以上のカード") and
/// plural-aware pronouns based on the pick count.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札を上から4枚まで見て、《NIKKE》のキャラを1枚まで選んで相手に見せ、手札に加え、残りのカードを控え室に置く</c> or <c>...残りのカードを山札の上に好きな順番で置く</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?自分の山札を上から(Ｘ|\d+)枚まで見て、(.+?)を(\d+)枚まで選(?:んで相手に見せ|び)、手札に加え、残りのカードを(?:控え室に置[くき]|山札の上に好きな順番で置く)(?:、|\.|,|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Look count (e.g., "4", "Ｘ")</description></item>
///   <item><description>Group 2: Card description (e.g., "《NIKKE》のキャラ", "レベル1以下のキャラ")</description></item>
///   <item><description>Group 3: Pick count (e.g., "1", "3", "Ｘ")</description></item>
/// </list>
/// <para><b>Output (atomic abilities):</b></para>
/// <list type="bullet">
///   <item><description><c>look at up to N cards from the top of your deck</c></description></item>
///   <item><description><c>choose up to N {description} from among them</c> (pluralized when pick &gt; 1)</description></item>
///   <item><description><c>reveal {it|them} to your opponent</c> (optional, only when <c>相手に見せ</c> present)</description></item>
///   <item><description><c>put {it|them} to your hand</c></description></item>
///   <item><description><c>put the rest to your waiting room</c></description></item>
/// </list>
/// <para><b>Rationale:</b> This token is registered before <c>LookAtTopCardsToken</c> to consume the full chain when present.
/// Atomic decomposition prevents double-conjunction issues when the parent joins with other preceding abilities.</para>
/// <para><b>Card description mapping:</b></para>
/// <list type="bullet">
///   <item><description>カード → card</description></item>
///   <item><description>レベルN以下のキャラ → level N or lower character</description></item>
///   <item><description>レベルN以上のカード → level N or higher card</description></item>
///   <item><description>黄のCX → yellow CX</description></item>
///   <item><description>CX → CX</description></item>
///   <item><description>《trait》のキャラ → &lt;&lt;trait&gt;&gt; character</description></item>
///   <item><description>Fallback: raw Japanese text</description></item>
/// </list>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different card descriptions (色のCX, 特定のカード名)
/// - Different follow-up actions after pick (手札に加える vs ストックに置く)
/// - Without reveal step (選び、 directly to 手札に加え)</para>
/// </remarks>
internal class LookAtDeckChooseCardRevealAddToHandRestToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の山札を上から(Ｘ|\d+)枚まで見て、(.+?)を(\d+)枚まで選(?:んで相手に見せ|び)、手札に加え、残りのカードを(?:控え室に置[くき]|山札の上に好きな順番で置く)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の山札を上から4枚まで見て、《★TESTTRAIT★》のキャラを1枚まで選んで相手に見せ、手札に加え、残りのカードを控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var lookCount = match.Groups[1].Value.Replace("Ｘ", "X");
        var cardDesc = match.Groups[2].Value;
        var pickCountRaw = match.Groups[3].Value;
        var pickCount = pickCountRaw.Replace("Ｘ", "X");
        var hasReveal = match.Value.Contains("相手に見せ");
        var isPlural = int.TryParse(pickCountRaw, out var pc) && pc > 1;
        var cardDescEnglish = cardDesc switch
        {
            _ when cardDesc == "カード" => "card",
            _ when cardDesc.Contains("黄のCX") => "yellow CX",
            _ when cardDesc.Contains("CX") => "CX",
            _ when Regex.Match(cardDesc, @"レベル(\d+)以下のキャラ") is Match lvlBelow && lvlBelow.Success => $"level {lvlBelow.Groups[1].Value} or lower character",
            _ when Regex.Match(cardDesc, @"レベル(\d+)以上のカード") is Match lvlAbove && lvlAbove.Success => $"level {lvlAbove.Groups[1].Value} or higher card",
            _ when Regex.Match(cardDesc, @"《(.+?)》のキャラ") is Match m && m.Success => $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>> character",
            _ when Regex.Match(cardDesc, @"《(.+?)》") is Match m && m.Success => $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>>",
            _ => cardDesc
        };
        var cardDescPlural = isPlural
            ? cardDescEnglish.Replace("character", "characters").Replace("card", "cards")
            : cardDescEnglish;
        var pickPhrase = $"choose up to {pickCount} {cardDescPlural} from among them";
        var pronoun = isPlural ? "them" : "it";
        var result = new List<CardEffectAbility>
        {
            new CardEffectAbility { AbilityText = $"look at up to {lookCount} cards from the top of your deck" },
            new CardEffectAbility { AbilityText = pickPhrase }
        };
        if (hasReveal)
            result.Add(new CardEffectAbility { AbilityText = $"reveal {pronoun} to your opponent" });
        result.Add(new CardEffectAbility { AbilityText = $"put {pronoun} to your hand" });
        var restAction = match.Value.Contains("山札の上に好きな順番で置く")
            ? "put the rest on the top of your deck in any order"
            : "put the rest to your waiting room";
        result.Add(new CardEffectAbility { AbilityText = restAction });
        return result;
    }
}
