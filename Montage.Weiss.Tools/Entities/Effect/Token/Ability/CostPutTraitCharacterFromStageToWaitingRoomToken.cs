namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CostPutTraitCharacterFromStageToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの舞台の《(.+?)》のキャラを(\d+)枚控え室に置(?:く|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches =>
    [
        "他のあなたの舞台の《★TESTTRAIT★》のキャラを1枚控え室に置く。",
        "他のあなたの舞台の《★TESTTRAIT★》のキャラを2枚控え室に置き"
    ];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} other <<{trait}>> character{(count != "1" ? "s" : "")} on stage to your waiting room"
            }
        ];
    }
}
