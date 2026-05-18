namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put top card of clock to waiting room" clauses (without subject prefix).
/// </summary>
internal class ClockToWaitingRoomSimpleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^クロックの上から(?<count>\d+)枚(?<upTo>まで)?を、?控え室に置(?<verb>いてよい|き|く)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups["count"].Value);
        var hasUpTo = match.Groups["upTo"].Success;
        var verb = match.Groups["verb"].Value;

        var isMay = verb == "いてよい" || hasUpTo;
        var countText = count == 1 ? "the top card" : $"the top {count} cards";
        var upToPhrase = countText + " of";

        return
        [
            new CardEffectAbility
            {
                AbilityText = isMay
                    ? $"you may put {upToPhrase} your clock to your waiting room"
                    : $"put {upToPhrase} your clock to your waiting room"
            }
        ];
    }
}
