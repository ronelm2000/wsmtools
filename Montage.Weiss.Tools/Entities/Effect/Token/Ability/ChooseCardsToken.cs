namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCardsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^カードを(?<count>Ｘ|\d+)枚まで選び(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var displayCount = count.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {displayCount} card"
            }
        ];
    }
}
