namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOtherCharactersBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたのキャラすべてに、パワーを＋(\d+)。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your other characters get +{power} power"
            }
        ];
    }
}
