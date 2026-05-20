namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PerformFollowingActionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の行動を行う。『(?<inner>.+?)』(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var inner = match.Groups["inner"].Value;
        var innerResult = PowerBoostWithFollowingAbilityToken.TryTranslateNested(registry, inner);
        return
        [
            new CardEffectAbility
            {
                AbilityText = innerResult != null
                    ? $"perform the following action. \"{innerResult}\""
                    : inner
            }
        ];
    }
}
