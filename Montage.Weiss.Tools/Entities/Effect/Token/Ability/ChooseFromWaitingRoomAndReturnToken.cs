namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseFromWaitingRoomAndReturnToken>();

    // Matches: あなたは自分の控え室の (【レベルX以下の】《...》の)?キャラを...枚選び、手札に戻す
    // Also matches: あなたは自分の控え室のトリガーアイコンが...の CX を...枚選び、手札に戻す
    // Also matches: あなたは自分の控え室の CX を...枚選び、手札に戻す (bare CX without trigger icon)
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?(?:自分の)?控え室の(?:(?:(?:レベル(?<level>[Ｘ\d]+)以下の)?《(.+?)》の)?キャラ|トリガーアイコンが\[\[(.+?)\]\]のCX|CX)を(\d+)枚(?:まで)?選び、(?<action>手札に戻してよい|手札に戻す|手札に戻し|このカードの下にマーカーとして表向きに置いてよい)(?:\.|,|、|。)?");

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
        var trait = match.Groups[1].Value;
        var triggerIcon = match.Groups[2].Value;
        // Strip .gif extension if present
        var triggerIconClean = triggerIcon.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) 
            ? triggerIcon[..^4] 
            : triggerIcon;
        var count = int.Parse(match.Groups[3].Value);
        var isUpTo = span.ToString().Contains("まで");
        var action = match.Groups["action"].Value;
        var mayText = action.EndsWith("てよい", StringComparison.Ordinal) || action.EndsWith("ていい", StringComparison.Ordinal) ? "you may " : "";
        
        var countText = isUpTo ? $"up to {count}" : count.ToString();
        var levelText = level != null ? $" level {level.Replace("Ｘ", "X")} or lower" : "";
        
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
                        AbilityText = $"put it face up underneath this card as a marker"
                    }
                ];
            }
            var traitTextForMarker = trait != null ? $" <<{trait}>>" : "";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {countText}{traitTextForMarker} character in your waiting room"
                },
                new CardEffectAbility
                {
                    AbilityText = $"put it face up underneath this card as a marker"
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
                    AbilityText = $"return it to your hand"
                }
            ];
        }

        if (string.IsNullOrEmpty(trait))
        {
            var hasCharacter = string.IsNullOrEmpty(triggerIcon) && match.Value.Contains("キャラ");
            var chooseText = hasCharacter
                ? $"{mayText}choose {countText} character in your waiting room"
                : $"{mayText}choose {countText} CX in your waiting room";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = chooseText
                },
                new CardEffectAbility
                {
                    AbilityText = $"return it to your hand"
                }
            ];
        }
        
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"{mayText}choose {countText}{levelText} <<{trait}>> character in your waiting room"
            },
            new CardEffectAbility
            {
                AbilityText = $"return it to your hand"
            }
        ];
    }
}
