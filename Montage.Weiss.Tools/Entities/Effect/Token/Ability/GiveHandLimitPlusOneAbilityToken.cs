namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveHandLimitPlusOneAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?キャラを1枚選び、(?:そのターン中、)?(?:そのターンの)?次のターンの終わりまで、次の能力を与える。『【永】\sあなたの手札の枚数上限を＋1。』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Choose 1 of your characters, and that character gets the following ability until the end of your next turn. \"[CONT] You get +1 to the limit on the number of cards in your hand.\""
            }
        ];
    }
}
