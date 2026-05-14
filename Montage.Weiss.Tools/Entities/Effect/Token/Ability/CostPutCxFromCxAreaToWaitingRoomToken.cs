namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCxFromCxAreaToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのCX置場の「(?<name>.+?)」を1枚控え室に置く");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups["name"].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put 1 \"{name}\" in your CX area to your waiting room"
            }
        ];
    }
}
