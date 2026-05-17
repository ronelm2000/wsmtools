namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostPerOtherNikkeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの《(.+?)》のキャラ1枚につき、このカードのパワーを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = match.Groups[1].Value;
            var power = match.Groups[2].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"This card gets +{power} power for each of your other <<{trait}>> characters"
                }
            ];
        }
        return [];
    }
}
