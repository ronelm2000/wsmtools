namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Look at top cards of deck" clauses with optional follow-up actions.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札を上から2枚まで見て、山札の上に好きな順番で置く</c> or <c>あなたは自分の山札を上から2枚まで見て、山札の上に好きな順番で置く</c> or <c>相手の山札を上から2枚まで見て、山札の上に元の順番で置く</c></para>
/// <para><b>Regex:</b> ^(?:あなたは(?:自分の|相手の)?|相手の)?山札を上から(Ｘ|\d+)枚まで見て(?:、(?&lt;follow&gt;山札の上に(?:好きな順番で|元の順番で)置く))?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count (e.g., "2" or "X")</description></item>
///   <item><description>Group "follow": Follow-up action (optional)</description></item>
/// </list>
/// <para><b>Output:</b> <c>look at up to 2 cards from the top of your deck, and put them on the top of your deck in any order</c></para>
/// </remarks>
internal class LookAtTopCardsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは(?:自分の|相手の)?|相手の)?山札を上から(Ｘ|\d+)枚まで見て(?:、(?<follow>山札の上に(?:好きな順番で|元の順番で)置く))?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var displayCount = count.Replace("Ｘ", "X");
        var fullText = span.ToString();
        var isOpponent = fullText.Contains("相手の山札") && !fullText.Contains("あなたは自分の山札");
        var deckOwner = isOpponent ? "your opponent's" : "your";
        var followUp = match.Groups["follow"].Value;

        string followUpText = string.Empty;
        if (!string.IsNullOrEmpty(followUp))
        {
            var pronoun = isOpponent ? "their" : "your";
            if (followUp.Contains("好きな順番で"))
            {
                followUpText = $", and put them on the top of {pronoun} deck in any order";
            }
            else if (followUp.Contains("元の順番で"))
            {
                followUpText = $", and put them on the top of {pronoun} deck in the original order";
            }
        }

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"look at up to {displayCount} cards from the top of {deckOwner} deck{followUpText}"
            }
        ];
    }
}
