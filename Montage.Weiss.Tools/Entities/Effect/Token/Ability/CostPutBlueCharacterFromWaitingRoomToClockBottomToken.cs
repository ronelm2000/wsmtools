namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutBlueCharacterFromWaitingRoomToClockBottomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたの控え室の青のキャラを1枚クロック置場の下に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Put 1 blue character in your waiting room at the bottom of your clock"
            }
        ];
    }
}
