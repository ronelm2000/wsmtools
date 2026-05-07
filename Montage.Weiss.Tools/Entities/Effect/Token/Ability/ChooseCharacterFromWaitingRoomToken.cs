namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterFromWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"自分の控え室のレベルＸ以下のキャラを1枚選び、手札に戻す。Ｘは公開されたカードのレベルに等しい。");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 level X or lower character from your waiting room, and return it into your hand. X is equal to the level of the revealed card"
            }
        ];
    }
}
