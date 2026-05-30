namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentBackRowLevelHigherThanOpponentAndNoStandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?相手の後列の、レベルが相手のレベルより高いキャラを1枚選ぶ\s*/\s*そのキャラは次の相手のスタンドフェイズに【スタンド】しない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 of your opponent's back stage characters with level higher than your opponent's level, and that character does not [STAND] during your opponent's next stand phase"
            }
        ];
    }
}
