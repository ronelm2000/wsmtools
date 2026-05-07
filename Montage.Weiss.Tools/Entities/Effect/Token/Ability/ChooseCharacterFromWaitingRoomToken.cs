using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseCharacterFromWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"自分の控え室のレベル(Ｘ|\d+)以下の(《.+?》のキャラ)?を(\d+)枚選び、(?<action>手札に戻す|舞台の好きな枠に置く)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var level = match.Groups[1].Value;
        var trait = match.Groups[2].Value;
        var count = match.Groups[3].Value;
        var action = match.Groups["action"].Value;

        var traitText = string.IsNullOrEmpty(trait) ? "" : $" {trait}";
        var actionText = action == "手札に戻す" ? "return it into your hand" : "put it on any slot on the stage";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} level {level} or lower{traitText} character from your waiting room, and {actionText}"
            }
        ];
    }
}
