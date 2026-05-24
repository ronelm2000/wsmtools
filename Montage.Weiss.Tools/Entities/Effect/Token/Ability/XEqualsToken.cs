namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class XEqualsToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[XＸ]\s*は\s*(?<description>.+?)\s*に等しい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["Xはあなたの《★TESTTRAIT★》のキャラの枚数に等しい。"];


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
            _ when description.Contains("そのキャラのソウル") =>
                "X is equal to that character's soul",
            _ when description.Contains("それらのカードの") =>
                $"X is equal to the number of {ExtractTrait(description, registry)} characters put this way",
            _ when description.Contains("あなたの") && description.Contains("キャラの枚数") =>
                $"X is equal to the number of your {(description.Contains("他の") ? "other " : "")}{ExtractTrait(description, registry)} characters",
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

    private static string ExtractTrait(string text, ITokenRegistry registry)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? $"<<{registry.MatchNameFragment(match.Groups[1].Value)}>>" : "?";
    }
}
