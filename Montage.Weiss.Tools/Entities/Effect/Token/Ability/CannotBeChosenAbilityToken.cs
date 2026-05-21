namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CannotBeChosenAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CannotBeChosenAbilityToken>();

    public override Regex Matcher => new(@"^このカードは相手の効果に選ばれない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        Log.Debug("CannotBeChosenAbilityToken: translating '{Span}'", span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "This card cannot be chosen by your opponent's effects."
            }
        ];
    }
}
