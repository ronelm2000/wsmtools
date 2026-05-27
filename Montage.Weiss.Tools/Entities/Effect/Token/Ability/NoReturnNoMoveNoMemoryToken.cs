namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class NoReturnNoMoveNoMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは手札に戻せず、他の枠に動かせず、思い出にできない(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card cannot be returned to hand, cannot be moved to other slots, and cannot be sent to memory"
            }
        ];
    }
}
