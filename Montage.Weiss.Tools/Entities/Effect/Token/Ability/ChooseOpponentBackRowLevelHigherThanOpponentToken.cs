namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentBackRowLevelHigherThanOpponentToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?相手の後列の、レベルが相手のレベルより高いキャラを1枚選ぶ(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 of your opponent's back stage characters with level higher than your opponent's level"
            }
        ];
    }
}
