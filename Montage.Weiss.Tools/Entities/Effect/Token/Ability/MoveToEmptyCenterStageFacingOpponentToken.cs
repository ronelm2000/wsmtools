namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MoveToEmptyCenterStageFacingOpponentToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードを、前列のキャラのいない枠で、正面に相手のキャラがいる枠に動かしてよい(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードを、前列のキャラのいない枠で、正面に相手のキャラがいる枠に動かしてよい"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may move this card to an empty center stage position in front of an opponent's character"
            }
        ];
    }
}
