namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ReverseThatCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのキャラを【リバース】してよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may [REVERSE] that character"
            }
        ];
    }
}
