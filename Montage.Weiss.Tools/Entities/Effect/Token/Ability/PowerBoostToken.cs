namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのパワーを[＋\+]([XＸ\d]+)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var powerStr = match.Groups[1].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{powerStr} power"
            }
        ];
    }
}
