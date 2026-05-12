namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ErosionGainToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは《浸食》を得る。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card gets <<Corruption>>"
            }
        ];
    }
}
