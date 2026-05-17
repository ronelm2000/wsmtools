namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MayPayCostThenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはコストを払ってよい。そうしたら、(?<effect>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var effectText = match.Groups["effect"].Value.Trim();

        // Use MultiClauseEffectParser for nested ability parsing
        var parsed = MultiClauseEffectParser.ParseSentence(effectText, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var abilityParts = parsed.Abilities.Select(a => a.AbilityText).ToList();

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
