namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class BrainstormEffectToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^集中\s*(?<rest>.+)$");

    public override CardEffect Translate(ITokenRegistry registry, Match match)
    {
        var rest = match.Groups["rest"].Value.Trim();
        var abilities = registry.EffectListRegistry.GetMatch(rest)(registry);
        var abilityEnglish = string.Join(", ", abilities.Select(a => a.AbilityText));

        return new AutoCardEffect
        {
            Labels = ["Brainstorm"],
            ConditionText = "Brainstorm",
            Condition = [new() { ConditionText = "Brainstorm" }],
            Cost = [],
            Abilities = abilities,
            AbilityText = abilityEnglish,
            EffectText = $"[AUTO][Brainstorm] {abilityEnglish}."
        };
    }
}
