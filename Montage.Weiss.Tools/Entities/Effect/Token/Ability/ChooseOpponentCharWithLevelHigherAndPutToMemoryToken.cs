namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentCharWithLevelHigherAndPutToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の、レベルが相手のレベルより高いキャラを1枚選び、思い出にする(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 of your opponent's characters with level higher than your opponent's level, and put it to their memory"
            }
        ];
    }
}
