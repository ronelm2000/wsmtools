namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character and give the following ability" clauses.
/// Supports optional "opponent's"/"your", "other", "battle" qualifiers, arbitrary duration, and translates inner quoted sub-abilities.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手のキャラを1枚選び、次の相手のターンの終わりまで、次の能力を与える。『【永】 このカードは他の枠に動かせない。』</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:他の)?(?:(?&lt;ownership&gt;相手の|自分の))?(?:バトル中の)?キャラを(?&lt;count&gt;\d+)枚選び、(?:(?&lt;duration&gt;そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?次の能力を与える。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Character count (e.g., "1")</description></item>
///   <item><description>ownership: "相手の" (opponent's) or "自分の" (your) — affects prefix text</description></item>
///   <item><description>duration: Optional duration text (e.g., "そのターン中", "次の相手のターンの終わりまで")</description></item>
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your opponent's characters, and that character gets the following ability until the end of your opponent's next turn. "[CONT] This card cannot move to another position of the stage."</c></para>
/// </remarks>
internal class ChooseOtherCharacterAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseOtherCharacterAndGiveAbilityToken>();

    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:あなたは)?(?:他の)?(?:(?<ownership>相手の|自分の))?(?:バトル中の)?キャラを(?<count>\d+)枚選び、(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?次の能力を与える。『(?<nested>.+)』");

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
        var prefix = match.Value[..match.Value.IndexOf('『')];
        var hasOther = prefix.Contains("他の", StringComparison.Ordinal);
        var isBattle = prefix.Contains("バトル中の", StringComparison.Ordinal);
        var ownership = match.Groups["ownership"].Value;
        var isOpponent = ownership == "相手の";
        var durationGroup = match.Groups["duration"];
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: count={Count}, nested='{Nested}', hasOther={HasOther}, isBattle={IsBattle}, isOpponent={IsOpponent}, duration='{Duration}'",
            count, nestedJapanese, hasOther, isBattle, isOpponent, durationGroup.Success ? durationGroup.Value : "none");
        
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        var countText = count == 1 ? "1" : count.ToString();
        var otherText = hasOther ? "other " : "";

        string choosePhrase;
        if (isBattle)
        {
            choosePhrase = ownership == "自分の"
                ? $"choose {countText} of your characters in battle"
                : $"choose {countText} character in battle";
        }
        else if (isOpponent)
        {
            choosePhrase = $"choose {countText} of your opponent's {otherText}characters";
        }
        else
        {
            choosePhrase = $"choose {countText} of your {otherText}characters";
        }

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        var result = $"{choosePhrase}, and that character gets the following ability{durationText}. \"{nestedEnglish}\"";
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
