namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostRestTraitCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの【スタンド】している《(.+?)》のキャラを 1 枚【レスト】する(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 1 of your other [STAND] <<{trait}>> characters"
            }
        ];
    }
}
