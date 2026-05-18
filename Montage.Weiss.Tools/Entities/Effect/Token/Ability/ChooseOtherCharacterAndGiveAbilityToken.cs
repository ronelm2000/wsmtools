namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOtherCharacterAndGiveAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<ChooseOtherCharacterAndGiveAbilityToken>();

    public override Regex Matcher => new(@"^(?:あなたは)?(?:他の)?(?:自分の)?キャラを(?<count>\d+)枚選び、そのターン中、次の能力を与える。『(?<nested>.+)』");

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
        Log.Debug("ChooseOtherCharacterAndGiveAbilityToken: count={Count}, nested='{Nested}'", count, nestedJapanese);
        
        var nestedEnglish = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, nestedJapanese) ?? nestedJapanese;

        var countText = count == 1 ? "1" : count.ToString();

        var result = $"choose {countText} of your other characters, and that character gets the following ability until end of turn. \"{nestedEnglish}\"";
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
