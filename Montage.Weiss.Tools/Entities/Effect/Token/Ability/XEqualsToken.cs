namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class XEqualsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^X は (?<description>.+?) に等しい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var description = match.Groups["description"] is Group g && g.Success ? g.Value : match.Value;
        var translated = description switch
        {
            _ when description.Contains("公開されたカードのレベル") =>
                "X is equal to the level of the revealed card",
            _ when description.Contains("そのカードのレベル＋1") =>
                "X is equal to that sent card's level +1",
            _ when description.Contains("それらのカードの") =>
                $"X is equal to the number of {ExtractTrait(description)} characters put this way",
            _ when description.Contains("あなたの") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of your {ExtractTrait(description)} characters",
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
        return match.Success ? $"<<{match.Groups[1].Value}>>" : "?";
    }
}
