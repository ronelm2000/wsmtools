namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllPlayersPerformActionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^すべてのプレイヤーは次の行動を行う");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "All players perform the following action"
            }
        ];
    }
}
