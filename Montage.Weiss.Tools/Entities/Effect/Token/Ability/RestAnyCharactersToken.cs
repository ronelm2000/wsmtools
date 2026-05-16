namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RestAnyCharactersToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたのキャラを(\d+)枚【レスト】する(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count = int.Parse(match.Groups[1].Value);
        return
        [
            new CardEffectAbility
            {
                AbilityText = count == 1
                    ? "[REST] 1 of your characters"
                    : $"[REST] {count} of your characters"
            }
        ];
    }
}
