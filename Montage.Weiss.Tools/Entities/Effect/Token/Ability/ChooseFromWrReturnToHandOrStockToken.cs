namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWrReturnToHandOrStockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:自分の)?控え室のレベル(Ｘ|\d+)以下の(《.+?》の)?キャラを(\d+)枚(?:まで)?選び、手札に戻すかストック置場に置[きく](?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の控え室のレベルＸ以下の《★TRAIT★》のキャラを1枚まで選び、手札に戻すかストック置場に置き"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = match.Groups[2].Value;
        var count = match.Groups[3].Value;
        var isUpTo = span.ToString().Contains("まで");

        var countText = isUpTo ? $"up to {count}" : count;
        var traitText = string.IsNullOrEmpty(trait) ? "" : $" <<{ExtractTrait(registry, trait)}>>";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText} level {level} or lower{traitText} character in your waiting room, and return it to your hand or put it to your stock"
            }
        ];
    }

    private static string ExtractTrait(ITokenRegistry registry, string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? registry.MatchNameFragment(match.Groups[1].Value) : "";
    }
}
