namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "deal X or Y damage" clauses (e.g., 相手に3ダメージか4ダメージを与える).
/// </summary>
internal class DealXorYDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?相手に(\d+)ダメージか(\d+)ダメージを与える(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手に3ダメージか4ダメージを与える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var dmg1 = match.Groups[1].Value;
        var dmg2 = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"deal {dmg1} or {dmg2} damage to your opponent"
            }
        ];
    }
}
