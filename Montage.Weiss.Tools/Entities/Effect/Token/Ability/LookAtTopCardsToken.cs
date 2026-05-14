namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class LookAtTopCardsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札を上から(Ｘ|\d+)枚まで見て(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        // Convert full-width Ｘ to half-width X for output
        var displayCount = count.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"look at up to {displayCount} cards from the top of your deck"
            }
        ];
    }
}
