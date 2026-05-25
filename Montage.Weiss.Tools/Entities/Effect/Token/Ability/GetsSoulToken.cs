namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "this card gets red" color attribute gain clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのターン中、このカードは赤を得る。</c></para>
/// <para><b>Regex:</b> ^(?:(?&lt;duration&gt;そのターン中)、)?このカードは赤を得る(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>duration: Optional "そのターン中、" prefix</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets red until end of turn</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different color attributes (青, 黄, etc.)</para>
/// </remarks>
internal class GetsSoulToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中)、)?このカードは赤を得る(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードは赤を得る。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var duration = match.Groups["duration"].Success ? " until end of turn" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets red{duration}"
            }
        ];
    }
}
