namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのキャラすべてに、パワーを＋(\d+)し、ソウルを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = int.Parse(match.Groups[1].Value);
        var soul = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your characters get +{power} power and +{soul} soul"
            }
        ];
    }
}
