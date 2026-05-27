namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CompoundDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に1ダメージを(\d+)回、(\d+)ダメージを(\d+)回、(\d+)ダメージを(\d+)回、(\d+)ダメージを順番に与える(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手に1ダメージを2回、2ダメージを2回、3ダメージを2回、4ダメージを順番に与える"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (!match.Success)
            return [];
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"deal 1 damage to your opponent {match.Groups[1].Value} times, {match.Groups[2].Value} damage {match.Groups[3].Value} times, {match.Groups[4].Value} damage {match.Groups[5].Value} times, and {match.Groups[6].Value} damage in order"
            }
        ];
    }
}
