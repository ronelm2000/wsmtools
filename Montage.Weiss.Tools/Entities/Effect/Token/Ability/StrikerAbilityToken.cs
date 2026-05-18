namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class StrikerAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^大活躍(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Great Performance"
            }
        ];
    }
}
