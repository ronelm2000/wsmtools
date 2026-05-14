namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTraitCharacterFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札の《(.+?)》のキャラを 1 枚控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 <<{trait}>> character in your hand to your waiting room"
            }
        ];
    }
}
