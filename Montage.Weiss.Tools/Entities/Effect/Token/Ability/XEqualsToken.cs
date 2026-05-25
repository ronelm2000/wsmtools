namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "X is equal to <description>" definition clauses with multiple known patterns.
/// Returns <see cref="UnmatchedAbility"/> when the description pattern is unrecognized.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>Xは公開されたカードのレベルに等しい。</c> or <c>Xはあなたの《NIKKE》のキャラの枚数に等しい。</c></para>
/// <para><b>Regex:</b> ^[XＸ]\s*は\s*(?&lt;description&gt;.+?)\s*に等しい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>description: The variable definition text</description></item>
/// </list>
/// <para><b>Output:</b> <c>X is equal to &lt;translated description&gt;</c></para>
/// <para><b>Scope Expansion:</b> To support new X definitions, add a new switch case in Translate() for the description pattern.</para>
/// </remarks>
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
        if (translated == description)
        {

            return [
                new UnmatchedAbility
                {
                    AbilityText = description,
                    IsUnmatched = true,
                    Suggestions = ["XEqualsToken couldn't match; create a new switch case."]
                }
            ];
        }
        else
        {
            return
            [
                new CardEffectAbility
            {
                AbilityText = translated
            }
            ];
        }
    }

    private static string ExtractTrait(string text, ITokenRegistry registry)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? $"<<{registry.MatchNameFragment(match.Groups[1].Value)}>>" : "?";
    }
}
