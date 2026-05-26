namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class OpponentFrontCharMinusSoulToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードの正面のキャラのソウルを[ー－\-](\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードの正面のキャラのソウルをー1。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var soul = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"the character in front of this card gets -{soul} soul"
            }
        ];
    }
}
