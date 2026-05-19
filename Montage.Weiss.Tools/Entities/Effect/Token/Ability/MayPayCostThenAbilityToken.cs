namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "you may pay the cost. If you do, &lt;effect&gt;" clauses, capturing and parsing the subsequent effect text.
/// Falls back to the raw effect text if no abilities are matched by <see cref="MultiClauseEffectParser"/>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたはコストを払ってよい。そうしたら、あなたは自分の山札を見て《NIKKE》のキャラを1枚まで選んで相手に見せ、手札に加え、その山札をシャッフルする。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?コストを払ってよい。そうしたら、(?&lt;effect&gt;.+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "effect": The full effect text after "そうしたら、"</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may pay the cost. If you do, search your deck for up to 1 &lt;&lt;NIKKE&gt;&gt; character, reveal it to your opponent, put it to your hand, and shuffle your deck</c></para>
/// <para><b>Notes:</b> Unlike <see cref="MayPayCostToken"/>, this token captures and translates the follow-up effect. A fallback emits the raw Japanese if parsing yields no abilities, preventing empty "If you do, ." output.</para>
/// </remarks>
internal class MayPayCostThenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<MayPayCostThenAbilityToken>();

    public override Regex Matcher => new(@"^(?:あなたは)?コストを払ってよい。そうしたら、(?<effect>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var effectText = match.Groups["effect"].Value.Trim().TrimStart('、');

        Log.Debug("MayPayCostThenAbilityToken: parsing effectText='{Text}'", effectText);

        var parsed = MultiClauseEffectParser.ParseSentence(effectText, registry, MultiClauseEffectParser.DefaultPrefixMap);
        var abilityParts = parsed.Abilities.Select(a => a.AbilityText).ToList();

        // Include post-conditions (e.g., X is equal to...) in the output
        var postConds = parsed.Conditions.Where(c => c.Type == ConditionType.PostCondition).ToList();
        if (postConds.Count > 0)
        {
            abilityParts.AddRange(postConds.Select(c => c.ConditionText));
        }

        // If no abilities were parsed, use the raw effect text as a fallback
        if (abilityParts.Count == 0 && !string.IsNullOrWhiteSpace(effectText))
        {
            abilityParts.Add(effectText);
        }

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
