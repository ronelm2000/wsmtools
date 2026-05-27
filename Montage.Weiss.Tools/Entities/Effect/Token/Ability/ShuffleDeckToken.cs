namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "shuffle your deck" clauses.
/// Supports both the plain form (シャッフルする) and "may" variant (シャッフルしてよい).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札をシャッフルする。</c> or <c>山札をシャッフルしてよい。</c></para>
/// <para><b>Regex:</b> ^山札をシャッフル(?:する|してよい)(?:\.|,|、|。)?</para>
/// <para><b>Output (plain):</b> <c>shuffle your deck</c></para>
/// <para><b>Output (may):</b> <c>you may shuffle your deck</c></para>
/// </remarks>
internal class ShuffleDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札をシャッフル(?:する|してよい)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["山札をシャッフルする。", "山札をシャッフルしてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isMay = match.Value.Contains("してよい");
        return
        [
            new CardEffectAbility
            {
                AbilityText = isMay ? "you may shuffle your deck" : "shuffle your deck"
            }
        ];
    }
}
