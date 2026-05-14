namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class AssistContEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^【永】\s*応援\s*(?<effect>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var effectText = match.Groups["effect"].Value.Trim();
        var abilities = registry.EffectListRegistry.GetMatch(effectText)(registry);
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        return new ContCardEffect
        {
            Labels = ["Assist"],
            Condition = [],
            ConditionText = "",
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = $"[CONT] Assist {abilityEnglish}."
        };
    }
}
