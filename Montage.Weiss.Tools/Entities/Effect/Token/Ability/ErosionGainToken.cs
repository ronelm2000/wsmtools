namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ErosionGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは《浸食》を得る。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card gets <<Corruption>>"
            }
        ];
    }
}
