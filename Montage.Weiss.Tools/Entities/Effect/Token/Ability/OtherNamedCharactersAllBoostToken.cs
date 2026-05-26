namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OtherNamedCharactersAllBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの「(.+?)」すべてに、パワーを[＋\+](\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var power = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your other \"{name}\" get +{power} power"
            }
        ];
    }
}
