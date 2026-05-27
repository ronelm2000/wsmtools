namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Look at top cards of deck" clauses with optional follow-up actions.
/// Supports "put on top in any/original order" as well as "put on top OR to waiting room" placement variants.
/// When a follow-up action is present, returns two separate abilities (look + put) for proper comma joining.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札を上から2枚まで見て、山札の上に好きな順番で置く</c> or <c>あなたは自分の山札を上から1枚見て、山札の上か控え室に置き</c></para>
/// <para><b>Regex:</b> ^(?:あなたは(?:自分の|相手の)?|相手の)?山札を上から(Ｘ|\d+)枚(?:まで)?見て(?:、(?&lt;follow&gt;山札の上に(?:好きな順番で|元の順番で)置く|山札の上か(?:下か)?控え室に置[くき]|山札の上か下に置く))?(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card count (e.g., "2" or "X")</description></item>
///   <item><description>Group "follow": Follow-up action (optional): order placement or OR placement</description></item>
/// </list>
/// <para><b>Output (no follow-up):</b> <c>look at [up to] N [cards|the top card] from the top of [your|your opponent's] deck</c></para>
/// <para><b>Output (with follow-up):</b> Two abilities: <c>look at ...</c> + <c>put it/them on top ...</c></para>
/// </remarks>
internal class LookAtTopCardsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは(?:自分の|相手の)?|相手の)?山札を上から(Ｘ|\d+)枚(?:まで)?見て(?:、(?<follow>山札の上に(?:好きな順番で|元の順番で)置く|山札の上か(?:下か)?控え室に置[くき]|山札の上か下に置く))?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var displayCount = count.Replace("Ｘ", "X");
        var fullText = span.ToString();
        var isOpponent = fullText.Contains("相手の山札") && !fullText.Contains("あなたは自分の山札");
        var deckOwner = isOpponent ? "your opponent's" : "your";
        var followUp = match.Groups["follow"].Value;
        var hasUpTo = fullText.Contains("まで");
        var countNumber = int.TryParse(count, out var parsedCount) ? parsedCount : 0;

        string lookText;
        if (countNumber == 1 && !hasUpTo)
        {
            lookText = "look at the top card of your deck";
        }
        else
        {
            lookText = $"look at up to {displayCount} cards from the top of {deckOwner} deck";
        }

        if (!string.IsNullOrEmpty(followUp))
        {
            var pronoun = isOpponent ? "their" : "your";
            string putText;
            if (followUp.Contains("上か下か控え室に"))
            {
                putText = $"put that card on the top of {pronoun} deck, the bottom of {pronoun} deck, or into your waiting room";
            }
            else if (followUp.Contains("上か控え室に"))
            {
                putText = $"put it on top of {pronoun} deck or to your waiting room";
            }
            else if (followUp.Contains("好きな順番で"))
            {
                putText = $"put them on the top of {pronoun} deck in any order";
            }
            else if (followUp.Contains("元の順番で"))
            {
                putText = $"put them on the top of {pronoun} deck in the original order";
            }
            else if (followUp.Contains("上か下に置く"))
            {
                putText = $"put it on the top of {pronoun} deck or the bottom of {pronoun} deck";
            }
            else
            {
                putText = "";
            }

            return
            [
                new CardEffectAbility
                {
                    AbilityText = lookText,
                    Prefix = AbilityPrefix.And
                },
                new CardEffectAbility
                {
                    AbilityText = putText,
                    Prefix = AbilityPrefix.And
                }
            ];
        }

        return
        [
            new CardEffectAbility
            {
                AbilityText = lookText,
                Prefix = AbilityPrefix.And
            }
        ];
    }
}
