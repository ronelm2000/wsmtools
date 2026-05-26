namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GrantPowerAndAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのパワーを[＋\+](\d+)し、このカードは次の(?<count>\d+)つの能力を得る。『(?<abil1>.+?)』『(?<abil2>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードのパワーを＋1000し、このカードは次の2つの能力を得る。『【自】 このカードがアタックした時、あなたは自分の山札を上から1枚見て、山札の上か下か控え室に置く。』『【自】 アンコール ［手札の《サマポケ》のキャラを1枚控え室に置く］』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        var count = match.Groups["count"].Value;
        var abil1 = match.Groups["abil1"].Value;
        var abil2 = match.Groups["abil2"].Value;

        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, abil1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, abil2);

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and the following abilities. \"{nestedEffect1.EffectText}\" \"{nestedEffect2.EffectText}\""
            }
        ];
    }
}
