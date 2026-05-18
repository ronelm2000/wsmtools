namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseTraitCharacterAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは(?:他の)?自分の《(.+?)》のキャラを(\d+)枚選び、そのターン中、パワーを＋(\d+)(?:\.|,|、|。)?");
    
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        var power = int.Parse(match.Groups[3].Value);
        var hasOther = match.Value.Contains("他の", StringComparison.Ordinal);
        var otherText = hasOther ? "other " : "";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your {otherText}<<{trait}>> characters, and that character gets +{power} power until end of turn"
            }
        ];
    }
}
