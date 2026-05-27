
namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "this card gets the following N abilities" clauses with two quoted sub-abilities.
/// Supports optional duration prefix like <c>次の相手のターンの終わりまで、</c>.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは次の2つの能力を得る。『【永】 あなたのターン中、このカードのパワーを＋5000。』『【自】 アンコール ［手札のキャラを1枚控え室に置く］』</c></para>
/// <para><b>Regex:</b> ^(?:&lt;duration&gt;...)?このカードは次の(?&lt;count&gt;\d+)つの能力を得る。『(?&lt;e1&gt;.+?)』『(?&lt;e2&gt;.+?)』</para>
/// </remarks>
internal class GainStandaloneFollowingAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードは次の(?<count>\d+)つの能力を得る。『(?<e1>.+?)』『(?<e2>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードは次の2つの能力を得る。『【永】 あなたのターン中、このカードのパワーを＋5000。』『【自】 アンコール ［手札の《★TESTTRAIT★》のキャラを1枚控え室に置く］』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var count = match.Groups["count"].Value;
        var e1 = match.Groups["e1"].Value;
        var e2 = match.Groups["e2"].Value;
        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, e2);

        var nestedEnglish1 = nestedEffect1.EffectText;
        if (!nestedEnglish1.EndsWith('.') && !nestedEnglish1.EndsWith('"') && !nestedEnglish1.EndsWith(']'))
            nestedEnglish1 += ".";

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
            durationText = dur;

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets the following abilities{durationText}. \"{nestedEnglish1}\" \"{nestedEffect2.EffectText}\"",
                NestedEffect = nestedEffect1,
                IsUnmatched = nestedEffect1.Abilities.Any(a => a.IsUnmatched) || nestedEffect2.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
