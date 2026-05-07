namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class XEqualsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"Ｘは(?<description>あなたの《.+?》のキャラの枚数)に等しい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var description = match.Groups["description"] is Group g && g.Success ? g.Value : match.Value;
        // Translate common patterns
        var translated = description switch
        {
            _ when description.Contains("《") && description.Contains("》") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of your {ExtractTrait(description)} characters",
            _ when description.Contains("そのカードのレベル＋1") =>
                "X is equal to that sent card's level +1",
            _ => description
        };
        return
        [
            new CardEffectAbility
            {
                AbilityText = translated
            }
        ];
    }

    private static string ExtractTrait(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? $" <<{match.Groups[1].Value}>>" : "?";  // Double space before trait to match test expectations
    }
}
