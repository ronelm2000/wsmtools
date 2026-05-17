namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveMultipleAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの「(?<cardName>.+?)」すべてに、次の(?<count>\d+)つの能力を与える。『(?<abil1>.+?)』『(?<abil2>.+?)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cardName = match.Groups["cardName"].Value;
        var abil1 = match.Groups["abil1"].Value;
        var abil2 = match.Groups["abil2"].Value;

        string nestedEnglish1, nestedEnglish2;
        try
        {
            var nestedEffect1 = registry.EffectRegistry.GetMatch(abil1.AsMemory())(registry);
            nestedEnglish1 = nestedEffect1.EffectText;
        }
        catch (NotImplementedException)
        {
            nestedEnglish1 = abil1;
        }

        try
        {
            var nestedEffect2 = registry.EffectRegistry.GetMatch(abil2.AsMemory())(registry);
            nestedEnglish2 = nestedEffect2.EffectText;
        }
        catch (NotImplementedException)
        {
            nestedEnglish2 = abil2;
        }

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All of your other \"{cardName}\" get the following abilities. \"{nestedEnglish1}\" \"{nestedEnglish2}\""
            }
        ];
    }
}
