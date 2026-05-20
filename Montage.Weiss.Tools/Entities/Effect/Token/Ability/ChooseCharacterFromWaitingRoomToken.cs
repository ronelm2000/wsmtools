namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character from waiting room" clauses with optional except-X prefix.
/// Supports level threshold (including X), trait qualifier, up-to count, and two actions (return to hand / put on stage).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは「"処断"D」以外の自分の控え室のレベル0以下の《NIKKE》のキャラを1枚選び、舞台の好きな枠に置く。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:「(?&lt;except&gt;[^」]+)」以外の)?自分の控え室のレベル(Ｘ|\d+)以下の(《.+?》の)?キャラを(\d+)枚(?:まで)?選び、(?&lt;action&gt;手札に戻す|舞台の好きな枠に置く)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>except: Optional card name to exclude (e.g., ""処断"D")</description></item>
///   <item><description>Group 1: Level (X or number)</description></item>
///   <item><description>Group 2: Trait (e.g., &lt;&lt;NIKKE&gt;&gt;)</description></item>
///   <item><description>Group 3: Character count</description></item>
///   <item><description>action: "手札に戻す" (return to hand) or "舞台の好きな枠に置く" (put on stage)</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 level 0 or lower &lt;&lt;NIKKE&gt;&gt; character except ""処断"D"" in your waiting room</c> + <c>put it on any position on your stage</c></para>
/// </remarks>
internal class ChooseCharacterFromWaitingRoomToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseCharacterFromWaitingRoomToken>();

    public override Regex Matcher => new(@"^(?:あなたは)?(?:「(?<except>[^」]+)」以外の)?自分の控え室のレベル(Ｘ|\d+)以下の(《.+?》の)?キャラを(\d+)枚(?:まで)?選び、(?<action>手札に戻す|舞台の好きな枠に置く)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var level = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = match.Groups[2].Value;
        var count = match.Groups[3].Value;
        var action = match.Groups["action"].Value;
        var exceptText = match.Groups["except"] is Group g && g.Success ? $" except \"{g.Value}\"" : "";
        var isUpTo = span.ToString().Contains("まで");

        Log.Debug("ChooseCharacterFromWaitingRoomToken: input='{Input}', level={Level}, trait='{Trait}', count={Count}, action='{Action}', except='{Except}'",
            span.ToString(), level, trait, count, action, exceptText);

        var countText = isUpTo ? $"up to {count}" : count;
        var traitText = string.IsNullOrEmpty(trait) ? "" : $" <<{ExtractTrait(trait)}>>";
        var isReturnToHand = action == "手札に戻す";
        var actionText = isReturnToHand ? "return it to your hand" : "put it on any position on your stage";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {countText} level {level} or lower{traitText} character{exceptText} in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = actionText
            }
        ];
    }

    private static string ExtractTrait(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"《(.+?)》");
        return match.Success ? match.Groups[1].Value : "";
    }
}
