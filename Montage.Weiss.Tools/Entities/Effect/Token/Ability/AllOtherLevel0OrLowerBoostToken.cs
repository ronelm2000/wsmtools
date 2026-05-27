namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOtherLevel0OrLowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?他のあなたのレベル0以下のキャラすべてに、パワーを＋(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"All your other level 0 or lower characters get +{power} power"
            }
        ];
    }
}
