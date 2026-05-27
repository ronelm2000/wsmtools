namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose WR character with level ≤ N to stock and give all characters soul boost" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の控え室のレベル1以下のキャラを1枚まで選び、ストック置場に置き、自分のキャラすべてに、そのターン中、ソウルを＋1。</c></para>
/// <para><b>Regex:</b> ^自分の控え室のレベル(\d+)以下のキャラを(\d+)枚まで選び、ストック置場に置き、自分のキャラすべてに、そのターン中、ソウルを＋(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Level threshold (e.g., <c>1</c>)</description></item>
///   <item><description>Group 2: Character count (e.g., <c>1</c>)</description></item>
///   <item><description>Group 3: Soul boost value (e.g., <c>1</c>)</description></item>
/// </list>
/// <para><b>Output:</b> Three atomic abilities: choose from WR, put to stock, and all-characters soul boost.</para>
/// </remarks>
internal class ChooseWrLevelXOrLowerToStockGiveAllSoulBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の控え室のレベル(\d+)以下のキャラを(\d+)枚まで選び、ストック置場に置き、自分のキャラすべてに、そのターン中、ソウルを＋(\d+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["自分の控え室のレベル1以下のキャラを1枚まで選び、ストック置場に置き、自分のキャラすべてに、そのターン中、ソウルを＋1。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        var soul = match.Groups[3].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} character with level {level} or less from your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = "put it into your stock"
            },
            new CardEffectAbility
            {
                AbilityText = $"all your characters get +{soul} soul until end of turn"
            }
        ];
    }
}
