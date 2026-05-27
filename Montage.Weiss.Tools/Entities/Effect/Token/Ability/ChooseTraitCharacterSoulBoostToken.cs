namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitCharacterSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^《(.+?)》のキャラを1枚選び、そのターン中、ソウルを[＋\+](\d+)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var soul = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 of your <<{trait}>> characters, and that character gets +{soul} soul until end of turn"
            }
        ];
    }
}
