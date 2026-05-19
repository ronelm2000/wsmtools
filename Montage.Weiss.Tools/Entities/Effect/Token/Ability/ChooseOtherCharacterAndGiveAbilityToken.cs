namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character and give the following ability" clauses.
/// Supports optional "other", "your", and "battle" qualifiers, and translates inner quoted sub-abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分のキャラを1枚選び、そのターン中、次の能力を与える。『【永】 このカードは【リバース】しない。』</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:他の)?(?:自分の)?(?:バトル中の)?キャラを(?&lt;count&gt;\d+)枚選び、そのターン中、次の能力を与える。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Character count (e.g., "1")</description></item>
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your characters, and that character gets the following ability until end of turn. "[CONT] This card cannot become [REVERSE]."</c></para>
/// </remarks>
internal class ChooseOtherCharacterAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseOtherCharacterAndGiveAbilityToken>();

    public override Regex Matcher => new(@"^(?:あなたは)?(?:他の)?(?:自分の)?(?:バトル中の)?キャラを(?<count>\d+)枚選び、そのターン中、次の能力を与える。『(?<nested>.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var input = span.ToString();
        var match = Matcher.Match(input);
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: input='{Input}', match.Success={Success}", input, match.Success);
        
        if (!match.Success)
        {
            Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: regex did not match");
            return [];
        }
        
        var count = int.Parse(match.Groups["count"].Value);
        var nestedJapanese = match.Groups["nested"].Value;
        var hasOther = match.Value.Contains("他の", StringComparison.Ordinal);
        var isBattle = match.Value.Contains("バトル中の", StringComparison.Ordinal);
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: count={Count}, nested='{Nested}', hasOther={HasOther}, isBattle={IsBattle}", count, nestedJapanese, hasOther, isBattle);
        
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        var countText = count == 1 ? "1" : count.ToString();
        var otherText = hasOther ? "other " : "";

        var choosePhrase = isBattle
            ? $"choose {countText} character in battle"
            : $"choose {countText} of your {otherText}characters";

        var result = $"{choosePhrase}, and that character gets the following ability until end of turn. \"{nestedEnglish}\"";
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: result='{Result}'", result);

        return
        [
            new CardEffectAbility
            {
                AbilityText = result
            }
        ];
    }
}
