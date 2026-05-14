namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのソウルを＋(\d+)");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var soul = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{soul} soul"
            }
        ];
    }
}
