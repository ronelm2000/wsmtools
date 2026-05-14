namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterFromWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?自分の控え室のレベル(Ｘ|\d+)以下の(《.+?》の)?キャラを(\d+)枚選び、(?<action>手札に戻す|舞台の好きな枠に置く)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = match.Groups[2].Value;
        var count = match.Groups[3].Value;
        var action = match.Groups["action"].Value;

        var traitText = string.IsNullOrEmpty(trait) ? "" : $" <<{ExtractTrait(trait)}>>";
        var isReturnToHand = action == "手札に戻す";
        var prep = isReturnToHand ? "in" : "in";
        var actionText = isReturnToHand ? "return it to your hand" : "put it on any position on your stage";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} level {level} or lower{traitText} character {prep} your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = actionText
            }
        ];
    }

    private static string ExtractTrait(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? match.Groups[1].Value : "";
    }
}
