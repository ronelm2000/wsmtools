namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "you may reveal up to N cards from the top of your deck" clauses with <c>してよい</c> (may) permission.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札の上から3枚までを、公開してよい。</c></para>
/// <para><b>Regex:</b> ^(?:自分の)?山札の上から(\d+)枚までを、公開してよい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Reveal count (e.g., <c>3</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>you may reveal up to N cards from the top of your deck</c></para>
/// <para><b>Usage:</b> Used in conditional reveal chains where the first sentence ends with <c>公開してよい。</c>
/// and a second sentence begins with a threshold condition like <c>N枚以上公開したなら</c>.</para>
/// </remarks>
internal class RevealUpToNAndMayToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:自分の)?山札の上から(\d+)枚までを、公開してよい(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分の山札の上から3枚までを、公開してよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may reveal up to {count} cards from the top of your deck"
            }
        ];
    }
}
