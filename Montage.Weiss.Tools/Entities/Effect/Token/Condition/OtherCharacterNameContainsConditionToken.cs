namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class OtherCharacterNameContainsConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^他のあなたのカード名に「(.+?)」を含むキャラがいるなら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectCondition
            {
                Type = ConditionType.If,
                ConditionText = $"there is another of your characters with \"{name}\" in its card name"
            }
        ];
    }
}
