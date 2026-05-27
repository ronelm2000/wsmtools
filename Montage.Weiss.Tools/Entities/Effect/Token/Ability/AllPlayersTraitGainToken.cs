namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllPlayersTraitGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?すべてのプレイヤーのキャラすべてに、そのターン中、《(.+?)》を与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All players' cahracters get <<{trait}>> to all players' characters until end of turn"
            }
        ];
    }
}
