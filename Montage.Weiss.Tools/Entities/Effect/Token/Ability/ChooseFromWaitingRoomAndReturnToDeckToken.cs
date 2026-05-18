namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の控え室のカードを(?<count>.+?)枚(?:まで)?選び、山札に戻し、その山札をシャッフルする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        var hasMade = match.Value.Contains("まで", StringComparison.Ordinal);
        var isSingular = count == "1";
        var upTo = hasMade ? "up to " : "";
        var cardNoun = isSingular ? "card" : "cards";
        var pronoun = isSingular ? "it" : "them";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {upTo}{count} {cardNoun} in your waiting room, return {pronoun} to your deck, and shuffle your deck"
            }
        ];
    }
}
