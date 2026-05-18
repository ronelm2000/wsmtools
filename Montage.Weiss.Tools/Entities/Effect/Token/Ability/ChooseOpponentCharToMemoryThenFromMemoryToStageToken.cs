namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentCharToMemoryThenFromMemoryToStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは相手のキャラを(?<count>.+?)枚(?:まで)?選び、思い出にし、相手は自分の思い出置場のそのキャラを、舞台の好きな枠に置く(?:\.|,|、|。)?");
    
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups["count"].Value;
        var hasMade = match.Value.Contains("まで", StringComparison.Ordinal);
        var upTo = hasMade ? "up to " : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {upTo}{count} of your opponent's characters, put it to their memory, and your opponent puts that character from their memory on any position of their stage"
            }
        ];
    }
}
