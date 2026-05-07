using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class TurnAndTraitCharacterCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"あなたのターン中、他のあなたの《(.+?)》のキャラが(\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectCondition
            {
                ConditionText = $"During your turn, if you have {count} or more other <<{trait}>> characters"
            }
        ];
    }
}
