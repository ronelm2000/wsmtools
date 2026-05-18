namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PutCharacterToClockToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはそのキャラをクロック置場に置いてよい(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put that character to your opponent's clock"
            }
        ];
    }
}
