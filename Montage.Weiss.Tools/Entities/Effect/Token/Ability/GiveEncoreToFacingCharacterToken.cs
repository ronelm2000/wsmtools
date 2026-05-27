namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class GiveEncoreToFacingCharacterToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^[、,]?(?:あなたは)?このカードの正面のキャラに、『【自】 アンコール ［\((\d+)\)］』を与える(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var cost = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"The character facing this card gets the following ability. \"[AUTO] Encore [({cost})]\""
            }
        ];
    }
}
