namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class XEqualsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^[XＸ]\s*は\s*(?<description>.+?)\s*に等しい(?:\.|,|、|。)?");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var description = match.Groups["description"] is Group g && g.Success ? g.Value : match.Value;
        var translated = description switch
        {
            _ when description.Contains("公開されたカードのレベル") =>
                "X is equal to the level of the revealed card",
            _ when Regex.IsMatch(description, @"そのカードのレベル[＋+]\d*") =>
                "X is equal to that sent card's level +1",
            _ when description.Contains("そのキャラのソウル") =>
                "X is equal to that character's soul",
            _ when Regex.IsMatch(description, @"そのキャラのレベル[×x]\d+") =>
                $"X is equal to that character's level {Regex.Match(description, @"[×x]\d+").Value}",
            _ when description.Contains("それらのカードの") =>
                $"X is equal to the number of {ExtractTrait(description)} characters put this way",
            _ when description.Contains("あなたの") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of your {ExtractTrait(description)} characters{FormatMultiplier(description)}",
            _ when description.Contains("相手の") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of characters your opponent has",
            _ => description
        };
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.PostCondition,
                ConditionText = translated
            }
        ];
    }

    private static string ExtractTrait(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? $"<<{match.Groups[1].Value}>>" : "?";
    }

    private static string FormatMultiplier(string description)
    {
        var multiplierMatch = Regex.Match(description, @"[×x]\d+$");
        return multiplierMatch.Success ? $" {multiplierMatch.Value}" : "";
    }
}
