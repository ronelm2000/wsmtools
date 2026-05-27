namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "reveal up to N cards from deck, choose up to X characters/events with a trait" clauses.
/// Supports <c>その中から</c> (from among them) optionally, as well as variant/card-based X counts.
/// Also handles <c>山札を上から</c> (with particle <c>を</c>) and alternative rest-card destinations
/// (<c>山札の上に好きな順番で置く</c> vs <c>控え室に置く</c>).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札の上から3枚まで公開し、《サマポケ》のキャラかイベントをＸ枚まで選び、手札に加え、残りのカードを控え室に置く</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?山札(?:の上から|を上から)(?&lt;lookCount&gt;Ｘ|\d+)枚まで公開し、(?&lt;fromAmong&gt;その中から)?(?&lt;cardDesc&gt;.+?)を(?&lt;pickCount&gt;[ＸX\d]+)枚まで選び、手札に加え、残りのカードを(?&lt;restAction&gt;山札の上に好きな順番で置く|控え室に置く)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>lookCount: Number of cards to reveal (e.g., "3" or "X")</description></item>
///   <item><description>fromAmong: Optional "その中から" prefix for "from among them"</description></item>
///   <item><description>cardDesc: Card type description (e.g., "《サマポケ》のキャラかイベント")</description></item>
///   <item><description>pickCount: Number of cards to pick (e.g., "X")</description></item>
///   <item><description>restAction: Destination for remaining cards — "山札の上に好きな順番で置く" (order on top) or "控え室に置く" (WR)</description></item>
/// </list>
/// <para><b>Output:</b> Four atomic abilities: reveal, choose [from among them], add to hand, and put rest to destination.</para>
/// <para><b>Plural handling:</b> Uses singular/plural pronouns ("it" vs "them") based on pick count. Picks variable "X" default to plural. </para>
/// </remarks>
internal class RevealUpToNFromDeckChooseTraitOrEventAddToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?山札(?:の上から|を上から)(?<lookCount>Ｘ|\d+)枚まで公開し、(?<fromAmong>その中から)?(?<cardDesc>.+?)を(?<pickCount>[ＸX\d]+)枚まで選び、手札に加え、残りのカードを(?<restAction>山札の上に好きな順番で置く|控え室に置く)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["山札の上から3枚まで公開し、《サマポケ》のキャラかイベントをＸ枚まで選び、手札に加え、残りのカードを控え室に置く。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var lookCount = match.Groups["lookCount"].Value.Replace("Ｘ", "X");
        var cardDesc = match.Groups["cardDesc"].Value;
        var pickCountRaw = match.Groups["pickCount"].Value;
        var pickCount = pickCountRaw.Replace("Ｘ", "X");
        var isPlural = int.TryParse(pickCountRaw, out var pc) && pc > 1;
        var isVariableX = pickCountRaw == "Ｘ" || pickCountRaw == "X";
        var effectivePlural = isPlural || isVariableX;
        var hasFromAmong = match.Groups["fromAmong"].Success;
        var restAction = match.Groups["restAction"].Value;
        var restActionText = restAction switch
        {
            "山札の上に好きな順番で置く" => "put the rest on the top of your deck in any order",
            "控え室に置く" => "put the rest to your waiting room",
            _ => $"put the rest {restAction}"
        };

        string TranslateCardDesc(string jp)
        {
            return jp switch
            {
                "キャラ" => "character",
                "イベント" => "event",
                "カード" => "card",
                _ => jp
            };
        }

        var cardDescEnglish = cardDesc switch
        {
            _ when Regex.Match(cardDesc, @"《(.+?)》の(.+?)か《(.+?)》の(.+)") is Match orMatch && orMatch.Success =>
                $"<<{registry.MatchNameFragment(orMatch.Groups[1].Value)}>> {TranslateCardDesc(orMatch.Groups[2].Value)} or <<{registry.MatchNameFragment(orMatch.Groups[3].Value)}>> {TranslateCardDesc(orMatch.Groups[4].Value)}",
            _ when Regex.Match(cardDesc, @"《(.+?)》のキャラか(.+)") is Match m && m.Success =>
                $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>> {TranslateCardDesc("キャラ")} or {TranslateCardDesc(m.Groups[2].Value)}",
            _ when Regex.Match(cardDesc, @"《(.+?)》のキャラ") is Match m && m.Success =>
                $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>> character",
            _ when Regex.Match(cardDesc, @"《(.+?)》") is Match m && m.Success =>
                $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>>",
            _ => cardDesc
        };

        var cardDescPlural = isPlural
            ? cardDescEnglish.Replace("character", "characters").Replace("event", "events").Replace("card", "cards")
            : cardDescEnglish;

        var chooseText = hasFromAmong
            ? $"choose up to {pickCount} {cardDescPlural} from among them"
            : $"choose up to {pickCount} {cardDescPlural}";
        var pronoun = effectivePlural ? "them" : "it";
        return
        [
            new CardEffectAbility { AbilityText = $"reveal up to {lookCount} cards from the top of your deck" },
            new CardEffectAbility { AbilityText = chooseText },
            new CardEffectAbility { AbilityText = $"add {pronoun} to your hand" },
            new CardEffectAbility { AbilityText = restActionText }
        ];
    }
}
