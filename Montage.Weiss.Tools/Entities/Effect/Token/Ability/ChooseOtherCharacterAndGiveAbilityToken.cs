namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a character and give the following ability" clauses.
/// Supports optional "opponent's"/"your", "other", trait prefix, "battle" qualifiers,
/// <c>枚まで</c> (up to N), arbitrary duration, and translates inner quoted sub-abilities.
/// Also supports the combined power boost + quoted ability grant pattern (e.g., <c>パワーを＋1500し、『...』を与える</c>).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手のキャラを1枚選び、次の相手のターンの終わりまで、次の能力を与える。『【永】 このカードは他の枠に動かせない。』</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:他の)?(?:(?&lt;ownership&gt;相手の|自分の))?(?:バトル中の)?(?:《(?&lt;trait&gt;.+?)》の)?キャラを(?&lt;count&gt;\d+)枚(?:まで)?選び、(?:(?&lt;duration&gt;そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?(?:次の能力を与える。『(?&lt;nested&gt;.+)』|パワーを[＋\+](?&lt;power&gt;\d+)し、『(?&lt;nested_power&gt;.+)』を与える)</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>count: Character count (e.g., "1" or "2")</description></item>
///   <item><description>ownership: "相手の" (opponent's) or "自分の" (your) — affects prefix text</description></item>
///   <item><description>trait: Optional trait in 《 》 (e.g., "サマポケ")</description></item>
///   <item><description>duration: Optional duration text (e.g., "そのターン中", "次の相手のターンの終わりまで")</description></item>
///   <item><description>nested / nested_power: Inner quoted ability text</description></item>
///   <item><description>power: Optional power boost value (for the combined boost+ability pattern)</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose N of your [other] [trait] characters, and [that character|those characters] gets [the following ability|+N power and the following ability][duration]. "..."</c></para>
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

    public override Regex Matcher => new(@"^(?:あなたは)?(?:他の)?(?:(?<ownership>相手の|自分の))?(?:バトル中の)?(?:《(?<trait>.+?)》の)?キャラを(?<count>\d+)枚(?:まで)?選び、(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?(?:次の能力を与える。『(?<nested>.+)』|パワーを[＋\+](?<power>\d+)し、『(?<nested_power>.+)』を与える)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches =>
    [
        "あなたは相手のキャラを1枚選び、そのターン中、次の能力を与える。『【永】 このカードは相手の効果に選ばれない。』",
        "他の自分の《サマポケ》のキャラを2枚まで選び、次の相手のターンの終わりまで、パワーを＋1500し、『【自】 アンコール ［手札のキャラを1枚控え室に置く］』を与える。"
    ];

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

        var hasNested = match.Groups["nested"].Success;
        var nestedJapanese = hasNested ? match.Groups["nested"].Value : match.Groups["nested_power"].Value;
        var prefix = match.Value[..match.Value.IndexOf('『')];
        var hasOther = prefix.Contains("他の", StringComparison.Ordinal);
        var isBattle = prefix.Contains("バトル中の", StringComparison.Ordinal);
        var ownership = match.Groups["ownership"].Value;
        var isOpponent = ownership == "相手の";
        var durationGroup = match.Groups["duration"];
        var trait = match.Groups["trait"].Success ? registry.MatchNameFragment(match.Groups["trait"].Value) : null;
        var powerGroup = match.Groups["power"];
        var powerValue = powerGroup.Success ? powerGroup.Value : null;
        var isUpTo = match.Value.Contains("枚まで");
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: count={Count}, trait='{Trait}', nested='{Nested}', hasOther={HasOther}, isBattle={IsBattle}, isOpponent={IsOpponent}, duration='{Duration}', power={Power}",
            match.Groups["count"].Value, trait ?? "none", nestedJapanese, hasOther, isBattle, isOpponent, durationGroup.Success ? durationGroup.Value : "none", powerValue ?? "none");
        
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);

        var count = int.Parse(match.Groups["count"].Value);
        var countText = isUpTo ? $"up to {count}" : (count == 1 ? "1" : count.ToString());
        var hasAnyOwnershipText = hasOther || (trait != null);

        string choosePhrase;
        if (isBattle)
        {
            choosePhrase = ownership == "自分の"
                ? $"choose {countText} of your characters in battle"
                : $"choose {countText} character in battle";
        }
        else if (isOpponent)
        {
            choosePhrase = hasOther
                ? $"choose {countText} of your opponent's other{(trait != null ? $" <<{trait}>>" : "")} characters"
                : $"choose {countText} of your opponent's characters";
        }
        else
        {
            choosePhrase = hasAnyOwnershipText
                ? $"choose {countText} of your{(hasOther ? " other" : "")}{(trait != null ? $" <<{trait}>>" : "")} characters"
                : $"choose {countText} of your characters";
        }

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        string result;
        if (powerValue != null)
        {
            result = $"{choosePhrase}, and those characters get +{powerValue} power and the following ability{durationText}. \"{nestedEffect.EffectText}\"";
        }
        else
        {
            result = $"{choosePhrase}, and that character gets the following ability{durationText}. \"{nestedEffect.EffectText}\"";
        }

        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: result='{Result}'", result);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = result,
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
