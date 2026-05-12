namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の 2 つの能力を与える");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following abilities"
            }
        ];
    }
}
