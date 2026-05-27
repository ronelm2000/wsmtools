namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose opponent cost 0 or less center stage character and put to waiting room" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の前列のコスト0以下のキャラを1枚選び、控え室に置いてよい。</c></para>
/// <para><b>Regex:</b> ^相手の前列のコスト(\d+)以下のキャラを(\d+)枚選び、控え室に置いてよい(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Cost threshold (e.g., <c>0</c>)</description></item>
///   <item><description>Group 2: Character count (e.g., <c>1</c>)</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose N of your opponent's cost X or less characters in their center stage</c> + <c>put it to your waiting room</c></para>
/// <para><b>Counterpart:</b> <see cref="OpponentCenterStageCost0OrLowerToBottomOfDeckToken"/> for the deck-bound variant.</para>
/// </remarks>
internal class OpponentCenterStageCost0OrLowerToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のコスト(\d+)以下のキャラを(\d+)枚選び、控え室に置いてよい(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["相手の前列のコスト0以下のキャラを1枚選び、控え室に置いてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your opponent's cost {cost} or less characters in their center stage"
            },
            new CardEffectAbility
            {
                AbilityText = "put it to your waiting room"
            }
        ];
    }
}
