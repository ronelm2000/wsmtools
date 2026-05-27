namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose characters/CX from your waiting room and return them to your hand (or place as markers)" clauses.
/// Supports trait filter, level restriction, trigger icon filter, and marker placement as alternatives to returning to hand.
/// Now supports variable X counts (Ｘ) in addition to numeric counts.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは自分の控え室のレベル1以下の《★TESTTRAIT★》のキャラを1枚選び、手札に戻す。</c></para>
/// <para><b>Regex:</b> ^[、,]?(?:あなたは)?(?:自分の)?控え室の(?:(?:(?:レベル(?&lt;level&gt;[Ｘ\d]+)以下の)?《(.+?)》の)?キャラ|トリガーアイコンが\[\[(.+?)\]\]のCX|CX)を([Ｘ\d]+)枚(?:まで)?選び、(?&lt;action&gt;手札に戻してよい|手札に戻す|手札に戻し|このカードの下にマーカーとして表向きに置いてよい)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>level: Level restriction (optional, e.g., "X" or "1")</description></item>
///   <item><description>Group 1: Trait in 《 》 (optional)</description></item>
///   <item><description>Group 2: Trigger icon (optional, e.g., "shot.gif")</description></item>
///   <item><description>Group 3: Card count (numeric or "X")</description></item>
///   <item><description>action: Action to perform: return to hand or place as marker</description></item>
/// </list>
/// <para><b>Output:</b> <c>(you may) choose [up to] N [level X or lower] [trait] character(s)/CX in your waiting room</c> + <c>return it/them to your hand</c></para>
/// </remarks>
internal class ChooseFromWaitingRoomAndReturnToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseFromWaitingRoomAndReturnToken>();

    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?控え室の(?:『(?<keyword>.+?)』を持つ)?(?:(?:(?:レベル(?<level>[Ｘ\d]+)以下の)?《(.+?)》の)?キャラ|トリガーアイコンが\[\[(.+?)\]\]のCX|CX)を([Ｘ\d]+)枚(?:まで)?選び、(?<action>手札に戻してよい|手札に戻す|手札に戻し|このカードの下にマーカーとして表向きに置いてよい)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["あなたは自分の控え室のレベル1以下の《★TESTTRAIT★》のキャラを1枚選び、手札に戻す。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var input = span.ToString();
        var match = Matcher.Match(input);
        Log.Debug("ChooseFromWaitingRoomAndReturnToken: input='{Input}', match.Success={Success}", input, match.Success);
        
        if (!match.Success)
        {
            Log.Debug("ChooseFromWaitingRoomAndReturnToken: regex did not match");
            return [];
        }
        
        var level = match.Groups["level"].Success ? match.Groups["level"].Value : null;
        var keyword = match.Groups["keyword"].Success ? registry.MatchLabels($"【{match.Groups["keyword"].Value}】").FirstOrDefault() : null;
        
        var trait = match.Groups[1].Success ? registry.MatchNameFragment(match.Groups[1].Value) : "";
        var triggerIcon = match.Groups[2].Value;
        // Strip .gif extension if present
        var triggerIconClean = triggerIcon.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) 
            ? triggerIcon[..^4] 
            : triggerIcon;
        var countRaw = match.Groups[3].Value;
        var isVariableX = countRaw == "Ｘ" || countRaw == "X";
        var count = isVariableX ? 0 : int.Parse(countRaw);
        var isUpTo = span.ToString().Contains("まで");
        var action = match.Groups["action"].Value;
        var mayText = action.EndsWith("てよい", StringComparison.Ordinal) || action.EndsWith("ていい", StringComparison.Ordinal) ? "you may " : "";
        var isPlural = isVariableX || count > 1;
        
        var countText = isUpTo ? (isVariableX ? "up to X" : $"up to {count}") : (isVariableX ? "X" : count.ToString());
        var levelText = level != null ? $" level {level.Replace("Ｘ", "X")} or lower" : "";
        var keywordText = keyword != null ? $" with \"{keyword}\"" : "";
        var pronoun = isPlural ? "them" : "it";
        
        if (action.Contains("マーカーとして", StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(triggerIconClean))
            {
                return
                [
                    new CardEffectAbility
                    {
                        AbilityText = $"{mayText}choose {countText} CX with [{triggerIconClean.ToUpper()}] in its trigger icon in your waiting room"
                    },
                    new CardEffectAbility
                    {
                        AbilityText = $"put {pronoun} face up underneath this card as a marker"
                    }
                ];
            }
            var traitTextForMarker = trait != null ? $" <<{trait}>>" : "";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {countText}{traitTextForMarker} {(isPlural ? "characters" : "character")}{keywordText} in your waiting room"
                },
                new CardEffectAbility
                {
                    AbilityText = $"put {pronoun} face up underneath this card as a marker"
                }
            ];
        }
        
        if (!string.IsNullOrEmpty(triggerIconClean))
        {
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {countText} CX with [{triggerIconClean.ToUpper()}] in its trigger icon in your waiting room"
                },
                new CardEffectAbility
                {
                    AbilityText = $"return {pronoun} to your hand"
                }
            ];
        }

        if (string.IsNullOrEmpty(trait))
        {
            var hasCharacter = string.IsNullOrEmpty(triggerIcon) && match.Value.Contains("キャラ");
            var chooseText = hasCharacter
                ? $"{mayText}choose {countText} {(isPlural ? "characters" : "character")}{keywordText} in your waiting room"
                : $"{mayText}choose {countText} CX in your waiting room";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = chooseText
                },
                new CardEffectAbility
                {
                    AbilityText = $"return {pronoun} to your hand"
                }
            ];
        }
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"{mayText}choose {countText}{levelText} <<{trait}>> {(isPlural ? "characters" : "character")}{keywordText} in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"return {pronoun} to your hand"
            }
        ];
    }
}
