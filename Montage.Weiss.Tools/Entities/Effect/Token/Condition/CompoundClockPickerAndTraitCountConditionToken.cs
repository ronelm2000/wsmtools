namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class CompoundClockPickerAndTraitCountConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたは自分のクロック置場のキャラを(\d+)枚まで選び、控え室に置き、他のあなたの《(.+?)》のキャラが(\d+)枚以上なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        var trait = registry.MatchNameFragment(match.Groups[2].Value);
        var threshold = match.Groups[3].Value;
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"you choose up to {count} character in your clock and put it into your waiting room, and you have {threshold} or more other <<{trait}>> characters"
            }
        ];
    }
}
