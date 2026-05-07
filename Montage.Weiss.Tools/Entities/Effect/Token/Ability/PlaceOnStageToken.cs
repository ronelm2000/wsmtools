using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PlaceOnStageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"舞台の好きな枠に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put it on any slot on the stage"
            }
        ];
    }
}
