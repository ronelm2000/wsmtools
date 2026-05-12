namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RestTraitCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの《(.+?)》のキャラを(\d+) 枚【レスト】する");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] {count} of your <<{trait}>> characters"
            }
        ];
    }
}
