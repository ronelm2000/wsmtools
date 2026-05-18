namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MayPayCostThenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MayPayCostThenAbilityToken>();

    public override Regex Matcher => new(@"^(?:あなたは)?コストを払ってよい。そうしたら、(?<effect>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var effectText = match.Groups["effect"].Value.Trim();

        Log.Debug("MayPayCostThenAbilityToken: parsing effectText='{Text}'", effectText);

        var parsed = MultiClauseEffectParser.ParseSentence(effectText, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var abilityParts = parsed.Abilities.Select(a => a.AbilityText).ToList();

        Log.Debug("MayPayCostThenAbilityToken: parsed {Count} abilities: {Abilities}",
            abilityParts.Count, string.Join(" | ", abilityParts));

        var joined = AutoEffectToken.JoinAbilityParts(abilityParts);
        if (joined.Length > 0)
            joined = char.ToLower(joined[0]) + joined[1..];
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may pay the cost. If you do, {joined}"
            }
        ];
    }
}
