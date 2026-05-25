namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put a card/character from your hand to your waiting room" clauses.
/// Supports <c>手札を</c> (card) and <c>手札のキャラを</c> (character) variants,
/// as well as <c>てよい</c> (you may) and plain <c>置く</c> forms.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>手札のキャラを1枚控え室に置く</c> or <c>手札を1枚控え室に置いてよい</c></para>
/// <para><b>Regex:</b> ^手札(?:のキャラ)?を(\d+)枚控え室に置(?:いてよい|く|き)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Card/character count (e.g., "1")</description></item>
/// </list>
/// <para><b>Output:</b> <c>(you may) put N (card(s)|character(s)) (in|from) your hand to your waiting room</c></para>
/// <para><b>Preposition logic:</b> Uses <c>from</c> for character cards (<c>のキャラ</c>), <c>in</c> for generic cards.</para>
/// </remarks>
internal class PutCardFromHandToWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^手札(?:のキャラ)?を(\d+)枚控え室に置(?:いてよい|く|き)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["手札のキャラを1枚控え室に置く。", "手札を1枚控え室に置いてよい。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        var noun = match.Value.Contains("のキャラ") ? "character" : "card";
        var useFrom = match.Value.Contains("のキャラ");
        var preposition = useFrom ? "from" : "in";
        var may = match.Value.Contains("てよい");
        var verb = may ? "Put" : "put";
        return
        [
            new CardEffectAbility
            {
                AbilityText = may
                    ? $"you may put {count} {(count == 1 ? noun : noun + "s")} {preposition} your hand to your waiting room"
                    : $"{verb} {count} {(count == 1 ? noun : noun + "s")} {preposition} your hand to your waiting room"
            }
        ];
    }
}
