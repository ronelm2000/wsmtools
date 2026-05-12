namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたは自分の控え室の(《(.+?)》の)?キャラを(\d+)枚選び、(?:手札に戻す|このカードの下にマーカーとして表向きに置いてよい)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[2].Success ? match.Groups[2].Value : null;
        var count = int.Parse(match.Groups[3].Value);
        var traitText = trait != null ? $" <<{trait}>>" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count}{traitText} character in your waiting room, and return it to your hand"
            }
        ];
    }
}
