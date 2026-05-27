namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "draw up to N cards" clauses with continuative verb form.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは1枚まで引き、そのターン中、このカードのパワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^あなたは(\d+)枚まで引き、</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Number of cards to draw</description></item>
/// </list>
/// <para><b>Output:</b> <c>draw up to 1 card</c></para>
/// </remarks>
internal class DrawUpToNToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(\d+)枚まで引き、|^(?:あなたは)?(\d+)枚まで引く(?:[\.\。,、]|\z)");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
        var noun = count == 1 ? "card" : "cards";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"draw up to {count} {noun}"
            }
        ];
    }
}
