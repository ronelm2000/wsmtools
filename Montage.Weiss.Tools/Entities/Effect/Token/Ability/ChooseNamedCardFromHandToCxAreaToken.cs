namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseNamedCardFromHandToCxAreaToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?手札の「(.+?)」を(\d+)枚まで選び、CX置場に置く(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups[1].Value);
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose up to {count} \"{name}\" from your hand and put it into your CX area"
            }
        ];
    }
}
