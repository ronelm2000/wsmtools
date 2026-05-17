namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "REST [STAND] character" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の【スタンド】しているレベル 3 以下のキャラを 1 枚選び、【レスト】し</c></para>
/// <para><b>Regex:</b> ^(.+?) の【スタンド】している(.+?) を 1 枚選び、【レスト】し (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Owner (e.g., "相手")</description></item>
///   <item><description>Group 2: Character description (e.g., "レベル 3 以下のキャラ")</description></item>
/// </list>
/// <para><b>Output:</b> <c>Choose 1 of your opponent's [STAND] level 3 or lower characters, [REST] it</c></para>
/// </remarks>
internal class RestStandCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(.+?)の【スタンド】している(.+?)を1枚選び、【レスト】し");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var owner = match.Groups[1].Value;
            var desc = match.Groups[2].Value;
            var ownerText = owner.Contains("相手") ? "your opponent's" : "your";
            var descText = desc
                .Replace("レベル3以下のキャラ", "level 3 or lower [STAND] characters")
                .Replace("レベル 3 以下のキャラ", "level 3 or lower [STAND] characters")
                .Replace("レベル0以下のキャラ", "level 0 or lower [STAND] characters")
                .Replace("レベル 0 以下のキャラ", "level 0 or lower [STAND] characters")
                .Replace("キャラ", "[STAND] characters");
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"Choose 1 of {ownerText} {descText}, [REST] it"
                }
            ];
        }
        return [];
    }
}
