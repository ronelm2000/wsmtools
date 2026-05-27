using Montage.Weiss.Tools.Entities.Effect.Token.Ability;

namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Strips 「『...』」 corner brackets and recursively parses the inner text as a card effect or ability.
/// Registered first in EffectRegistry to intercept quoted sub-abilities before EventEffectToken catches them.
/// Wraps the parsed output in double quotes to match CSV convention for nested sub-effects.
/// Falls back to EffectListRegistry if EffectRegistry matching fails.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>『あなたは1枚引く。』</c> or <c>『あなたは自分の山札をシャッフルしてよい。』</c></para>
/// <para><b>Regex:</b> ^(?:『)(.*)(?:』)$</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Inner text between 『』 (e.g., "あなたは1枚引く。")</description></item>
/// </list>
/// <para><b>Output:</b> <c>"Draw 1 card."</c> (wraps recursively parsed effect in quotes)</para>
/// <para><b>Processing:</b></para>
/// <list type="bullet">
///   <item><description>Strips 『』 brackets from input</description></item>
///   <item><description>Tries EffectRegistry.Match for full card effect parsing</description></item>
///   <item><description>Falls back to EffectListRegistry.Match for ability-only text</description></item>
///   <item><description>Returns EventCardEffect with wrapped EffectText</description></item>
/// </list>
/// </remarks>
internal class SubAbilityToken : CardTextToken<CardEffect>
{
    public override Regex Matcher => new(@"^(?:『)(.*)(?:』)$");
    public override IEnumerable<string> SampleMatches => ["『あなたは1枚引く。』", "『あなたは自分の山札をシャッフルしてよい。』"];

    public override CardEffect Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var innerText = match.Groups[1].Value.Trim();

        var effectMatch = registry.EffectRegistry.Match(innerText.AsMemory());
        if (effectMatch != null)
        {
            var innerEffect = effectMatch.Translate(registry);
            return new EventCardEffect
            {
                Labels = [],
                EffectText = $"\"{innerEffect.EffectText}\"",
                AbilityText = $"\"{innerEffect.EffectText}\"",
                Abilities = innerEffect.Abilities
            };
        }

        var abilMatch = registry.EffectListRegistry.Match(innerText.AsMemory());
        if (abilMatch != null)
        {
            var abils = abilMatch.Translate(registry);
            var abilityEnglish = string.Join(", ", abils.Select(a => a.AbilityText));
            return new EventCardEffect
            {
                Labels = [],
                EffectText = $"\"{abilityEnglish}\"",
                AbilityText = $"\"{abilityEnglish}\"",
                Abilities = abils
            };
        }

        return new EventCardEffect
        {
            Labels = [],
            EffectText = innerText,
            AbilityText = innerText,
            Abilities = [new UnmatchedAbility
            {
                AbilityText = innerText,
                IsUnmatched = true,
                Suggestions = ["unmatched nested ability text"]
            }]
        };
    }
}
