using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class BrainstormToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^集中\s+(?<rest>.+)$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var rest = match.Groups["rest"].Value.Trim();
        var abilities = registry.EffectListRegistry.GetMatch(rest)(registry);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Brainstorm. {string.Join(", ", abilities.Select(a => a.AbilityText))}"
            }
        ];
    }
}
