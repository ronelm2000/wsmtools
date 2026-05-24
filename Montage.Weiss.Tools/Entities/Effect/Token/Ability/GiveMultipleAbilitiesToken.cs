namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveMultipleAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<cardName>.+?)」すべてに、次の(?<count>\d+)つの能力を与える。『(?<abil1>.+?)』『(?<abil2>.+?)』");

    public override IEnumerable<string> SampleMatches => ["他のあなたの「★TESTNAME★」すべてに、次の2つの能力を与える。『能力1』『能力2』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cardName = registry.MatchNameFragment(match.Groups["cardName"].Value);
        var abil1 = match.Groups["abil1"].Value;
        var abil2 = match.Groups["abil2"].Value;

        string nestedEnglish1, nestedEnglish2;
        var effMatch1 = registry.EffectRegistry.Match(abil1.AsMemory());
        nestedEnglish1 = effMatch1?.Translate(registry).EffectText ?? abil1;

        var effMatch2 = registry.EffectRegistry.Match(abil2.AsMemory());
        nestedEnglish2 = effMatch2?.Translate(registry).EffectText ?? abil2;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your other \"{cardName}\" get the following abilities. \"{nestedEnglish1}\" \"{nestedEnglish2}\""
            }
        ];
    }
}
