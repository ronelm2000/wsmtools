namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたは自分の山札(?:を上から(.+?)枚まで見て、その中から|を見て)(《(.+?)》のキャラ|(.+?)を)?(.+?)枚まで選んで相手に見せ");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups[3].Success ? match.Groups[3].Value : match.Groups[4].Value;
        var pickCount = match.Groups[5].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {pickCount} <<{trait}>> character, reveal it to your opponent"
            }
        ];
    }
}

internal class SearchDeckWithTopLookToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"あなたは自分の山札を上から(.+?)枚まで見て、その中から《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var count = match.Groups[1].Value.Replace("Ｘ", "X");
        var trait = match.Groups[2].Value;
        var pickCount = match.Groups[3].Value.Replace("Ｘ", "X");
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} cards, choose up to {pickCount} <<{trait}>> character from among them, reveal it to your opponent"
            }
        ];
    }
}
