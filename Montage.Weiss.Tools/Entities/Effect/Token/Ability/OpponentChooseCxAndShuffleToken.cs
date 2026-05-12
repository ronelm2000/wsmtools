namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentChooseCxAndShuffleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"相手は自分の控え室のCXを(\d+)枚選び、(?<rest>.+)山札に戻し、(?:その山札を)?シャッフルする");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = int.Parse(match.Groups[1].Value);
        var rest = match.Groups["rest"].Value.Trim();
        // Translate optional intervening text (e.g., "それ以外のカードすべてを")
        var restEnglish = rest switch
        {
            _ when rest.Contains("それ以外のカードすべてを") => "returns all cards except that card from their waiting room to their deck",
            _ when rest.Contains("そのカード以外の") => "returns all cards except that card from their waiting room to their deck",
            _ => "returns cards from their waiting room to their deck"
        };
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"your opponent chooses {count} CX in their waiting room, {restEnglish}, and shuffles their deck"
            }
        ];
    }
}
