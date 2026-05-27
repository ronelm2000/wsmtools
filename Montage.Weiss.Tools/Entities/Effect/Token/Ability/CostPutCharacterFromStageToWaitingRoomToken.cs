namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutCharacterFromStageToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの舞台のキャラを(\d+)枚控え室に置(?:く|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches =>
    [
        "他のあなたの舞台のキャラを1枚控え室に置く。",
        "他のあなたの舞台のキャラを2枚控え室に置き"
    ];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} other character on stage to your waiting room"
            }
        ];
    }
}
