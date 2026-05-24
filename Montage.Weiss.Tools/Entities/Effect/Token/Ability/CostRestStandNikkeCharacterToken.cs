namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST 1 of your other [STAND] NIKKE characters" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>他のあなたの【スタンド】している《NIKKE》のキャラを 1 枚【レスト】する</c></para>
/// <para><b>Regex:</b> ^他のあなたの【スタンド】している《NIKKE》のキャラを 1 枚【レスト】する (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>[REST] 1 of your other [STAND] &lt;&lt;NIKKE&gt;&gt; characters</c></para>
/// </remarks>
internal class CostRestStandNikkeCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^他のあなたの【スタンド】している《NIKKE》のキャラを1枚【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["他のあなたの【スタンド】している《NIKKE》のキャラを1枚【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 1 of your other [STAND] <<{registry.MatchNameFragment("NIKKE")}>> characters"
            }
        ];
    }
}
