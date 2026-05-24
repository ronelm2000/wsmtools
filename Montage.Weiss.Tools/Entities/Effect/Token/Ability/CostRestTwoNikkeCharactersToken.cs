namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST 2 NIKKE characters" cost clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの《NIKKE》のキャラを 2 枚【レスト】する</c></para>
/// <para><b>Regex:</b> ^あなたの《NIKKE》のキャラを 2 枚【レスト】する (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>[REST] 2 of your &lt;&lt;NIKKE&gt;&gt; characters</c></para>
/// </remarks>
internal class CostRestTwoNikkeCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの《NIKKE》のキャラを2枚【レスト】する(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["あなたの《NIKKE》のキャラを2枚【レスト】する。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"[REST] 2 of your <<{registry.MatchNameFragment("NIKKE")}>> characters"
            }
        ];
    }
}
