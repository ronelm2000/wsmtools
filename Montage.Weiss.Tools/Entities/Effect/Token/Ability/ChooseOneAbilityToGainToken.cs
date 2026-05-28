namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOneAbilityToGainToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "次の相手のターンの終わりまで、", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
        { "そのターン中、", " until end of turn" },
        { "このターン中、", " until end of turn" },
    };

    public override Regex Matcher => new(@"^(?<dur>次の相手のターンの終わりまで、|次の相手のターンの終了時まで|そのターン中、|このターン中、)?[、,]?(?:あなたは)?このカードは次の2つの能力のうちあなたが選んだ1つを得る。『(?<abil1>.+?)』『(?<abil2>.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["次の相手のターンの終わりまで、このカードは次の2つの能力のうちあなたが選んだ1つを得る。『【自】 相手のアタックフェイズの始めに...』『【自】［(1) このカードを控え室に置く］...』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var ability1 = match.Groups["abil1"].Value;
        var ability2 = match.Groups["abil2"].Value;
        var durCapture = match.Groups["dur"].Success ? match.Groups["dur"].Value : null;
        var durText = durCapture != null && DurationMap.TryGetValue(durCapture, out var d) ? d : "";
        var translated1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, ability1);
        var translated2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, ability2);
        var abilityText = $"this card gets 1 of the following abilities of your choice{durText}. \"{translated1.EffectText}\" \"{translated2.EffectText}\"";
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = abilityText,
                NestedEffect = translated1,
                IsUnmatched = translated1.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
