namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOtherTraitCharPowerAndGrantAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?他の自分の《(.+?)》のキャラを(\d+)枚選び、次の相手のターンの終わりまで、パワーを[＋\+](\d+)し、次の能力を与える。『(.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = int.Parse(match.Groups[2].Value);
        var power = match.Groups[3].Value;
        var nestedJapanese = match.Groups[4].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {count} of your other <<{trait}>> characters, and that character gets +{power} power until the end of your opponent's next turn, and gets the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
