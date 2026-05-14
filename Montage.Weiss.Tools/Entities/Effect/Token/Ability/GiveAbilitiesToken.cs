namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の 2 つの能力を与える");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following abilities"
            }
        ];
    }
}
