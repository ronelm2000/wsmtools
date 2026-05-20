namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class MoveOpponentCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のキャラを(\d+)枚選び、相手の舞台のキャラのいない他の枠に動かす(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your opponent's characters"
            },
            new CardEffectAbility
            {
                AbilityText = "move it to another open position of their stage"
            }
        ];
    }
}
