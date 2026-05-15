namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTraitCharacterFromStageToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの舞台の《(.+?)》のキャラを\s*1\s*枚控え室に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 other <<{trait}>> character in your stage to your waiting room"
            }
        ];
    }
}
