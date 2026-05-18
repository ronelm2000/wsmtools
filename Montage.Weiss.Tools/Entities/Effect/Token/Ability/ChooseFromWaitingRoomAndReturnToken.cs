namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseFromWaitingRoomAndReturnToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseFromWaitingRoomAndReturnToken>();

    // Matches: あなたは自分の控え室の (《...》の)?キャラを...枚選び、手札に戻す
    // Also matches: あなたは自分の控え室のトリガーアイコンが...の CX を...枚選び、手札に戻す
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?自分の控え室の(?:(?:《(.+?)》の)?キャラ|トリガーアイコンが\[\[(.+?)\]\]のCX)を(\d+)枚選び、(?<action>手札に戻す|このカードの下にマーカーとして表向きに置いてよい)");

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
        
        var trait = match.Groups[1].Value;
        var triggerIcon = match.Groups[2].Value;
        // Strip .gif extension if present
        var triggerIconClean = triggerIcon.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) 
            ? triggerIcon[..^4] 
            : triggerIcon;
        var count = int.Parse(match.Groups[3].Value);
        var action = match.Groups["action"].Value;
        var mayText = action.EndsWith("てよい", StringComparison.Ordinal) || action.EndsWith("ていい", StringComparison.Ordinal) ? "you may " : "";
        
        if (action.Contains("マーカーとして", StringComparison.Ordinal))
        {
            if (!string.IsNullOrEmpty(triggerIconClean))
            {
                return
                [
                    new CardEffectAbility
                    {
                        AbilityText = $"{mayText}choose {count} CX with [{triggerIconClean.ToUpper()}] in its trigger icon in your waiting room, and put it face up underneath this card as a marker"
                    }
                ];
            }
            var traitTextForMarker = trait != null ? $" <<{trait}>>" : "";
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {count}{traitTextForMarker} character in your waiting room, and put it face up underneath this card as a marker"
                }
            ];
        }
        
        if (!string.IsNullOrEmpty(triggerIconClean))
        {
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"{mayText}choose {count} CX with [{triggerIconClean.ToUpper()}] in its trigger icon in your waiting room, and return it to your hand"
                }
            ];
        }
        
        var traitTextForReturn = trait != null ? $" <<{trait}>>" : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"{mayText}choose {count}{traitTextForReturn} character in your waiting room, and return it to your hand"
            }
        ];
    }
}
