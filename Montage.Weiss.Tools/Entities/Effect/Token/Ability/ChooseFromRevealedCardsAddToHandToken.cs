namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose from among the revealed cards, add to hand, put rest in WR" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>それらのカードの《幻想郷》のキャラを1枚まで選び、手札に加え、残りのカードを控え室に置き。</c></para>
/// <para><b>Regex:</b> ^それらのカードの(.+?)を(\d+)枚(?:まで)?選び、手札に加え、残りのカードを控え室に置き(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card description (e.g., <c>《幻想郷》のキャラ</c>)</description></item>
///   <item><description>Group 2: Pick count (e.g., <c>1</c>)</description></item>
/// </list>
/// <para><b>Output:</b> Three atomic abilities: choose from among them, add to hand, put rest in WR.</para>
/// <para><b>Usage:</b> Follows a reveal action in a conditional chain (e.g., "if you revealed 1 or more cards, choose...").</para>
/// </remarks>
internal class ChooseFromRevealedCardsAddToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^それらのカードの(.+?)を(\d+)枚(?:まで)?選び、手札に加え、残りのカードを控え室に置き(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["それらのカードの《幻想郷》のキャラを1枚まで選び、手札に加え、残りのカードを控え室に置き。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cardDesc = match.Groups[1].Value;
        var count = match.Groups[2].Value;

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
            _ when Regex.Match(cardDesc, @"《(.+?)》のキャラ") is Match m && m.Success =>
                $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>> character",
            _ when Regex.Match(cardDesc, @"《(.+?)》") is Match m && m.Success =>
                $"<<{registry.MatchNameFragment(m.Groups[1].Value)}>>",
            _ => cardDesc
        };

        return
        [
            new CardEffectAbility { AbilityText = $"choose up to {count} {cardDescEnglish} from among them" },
            new CardEffectAbility { AbilityText = "add it to your hand" },
            new CardEffectAbility { AbilityText = "put the rest in your waiting room" }
        ];
    }
}
