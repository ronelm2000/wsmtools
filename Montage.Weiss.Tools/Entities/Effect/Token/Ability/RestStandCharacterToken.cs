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
/// <para><b>Output:</b> <c>Choose 1 of your opponent's level 3 or lower [STAND] characters, [REST] it</c></para>
/// <para><b>Note:</b> The [STAND] descriptor follows the level / character description
/// (e.g., <c>level 3 or lower [STAND] characters</c>), not before it.</para>
/// </remarks>
internal class RestStandCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(相手|あなた)の【スタンド】している(.+?)を1枚選び、【レスト】(?:し|する)(?:てよい)?(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var owner = match.Groups[1].Value;
            var desc = match.Groups[2].Value;
            var ownerText = owner == "相手" ? "your opponent's" : "your";
            var normalizedDesc = desc.Replace(" ", "");
            var descText = normalizedDesc switch
            {
                _ when normalizedDesc.Contains("レベル3以下のキャラ") => "level 3 or lower characters",
                _ when normalizedDesc.Contains("レベル0以下のキャラ") => "level 0 or lower characters",
                _ when normalizedDesc.Contains("レベルX以下のキャラ") => "level X or lower characters",
                _ when normalizedDesc.Contains("レベル") => normalizedDesc.Replace("レベル", "level ").Replace("以下のキャラ", " or lower characters"),
                _ => normalizedDesc.Replace("キャラ", "characters")
            };
            var standDescText = descText.EndsWith(" characters")
                ? descText[..^" characters".Length] + " [STAND] characters"
                : $"[STAND] {descText}";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"choose 1 of {ownerText} {standDescText}"
                },
                new CardEffectAbility
                {
                    AbilityText = $"[REST] it"
                }
            ];
        }
        return [];
    }
}
