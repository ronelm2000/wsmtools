namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ThatCharacterNoStandNextOpponentStandPhaseToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのキャラは次の相手のスタンドフェイズに【スタンド】しない(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "that character does not [STAND] during your opponent's next stand phase"
            }
        ];
    }
}
