namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Choose 1 trait character from waiting room and put to stock" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の控え室の《NIKKE》のキャラを 1 枚選び、ストック置場に置いてよい。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?自分の控え室の (《.+?》の)?キャラを (\d+) 枚選び、ストック置場に置いてよい</para>
/// <para><b>Output:</b> <c>you may choose 1 <<NIKKE>> character in your waiting room, and put it to your stock</c></para>
/// </remarks>
internal class ChooseTraitCharacterFromWrAndPutToStockToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseTraitCharacterFromWrAndPutToStockToken>();

    // Matches: 、あなたは自分の控え室の《NIKKE》のキャラを 1 枚選び、ストック置場に置いてよい。
    // Also matches: 自分の控え室の《NIKKE》のキャラを 1 枚選び、ストック置場に置いてよい。 (without prefix)
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?自分の控え室の(《.+?》の)?キャラを(\d+)枚選び、ストック置場に置いてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["自分の控え室の《★TESTTRAIT★》のキャラを1枚選び、ストック置場に置いてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var input = span.ToString();
        Log.Debug("ChooseTraitCharacterFromWrAndPutToStockToken: Translate called with input='{Input}'", input);
        var match = Matcher.Match(input);
        Log.Debug("ChooseTraitCharacterFromWrAndPutToStockToken: input='{Input}', match.Success={Success}, match.Length={Length}", input, match.Success, match.Length);
        
        if (!match.Success)
        {
            Log.Debug("ChooseTraitCharacterFromWrAndPutToStockToken: regex did not match. Pattern: {Pattern}", Matcher.ToString());
            return [];
        }
        
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        Log.Debug("ChooseTraitCharacterFromWrAndPutToStockToken: trait='{Trait}', count={Count}", trait, count);
        
        var traitText = string.IsNullOrEmpty(trait) ? "" : $"<<{ExtractTrait(trait, registry)}>> ";
        var countText = count == "1" ? "1" : count;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose {countText} {traitText}character in your waiting room, and put it to your stock"
            }
        ];
    }

    private static string ExtractTrait(string text, ITokenRegistry registry)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? registry.MatchNameFragment(match.Groups[1].Value) : "";
    }
}
