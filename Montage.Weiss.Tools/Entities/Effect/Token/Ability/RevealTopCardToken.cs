namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "reveal the top card of your deck" clauses.
/// Supports optional <c>あなたは</c> and <c>自分の</c> prefixes (stripped by <c>SkippablePrefixes</c> in nested contexts).
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札の上から1枚を公開する。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:自分の)?山札の上から1枚を公開(?:し|する)(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>reveal the top card of your deck</c></para>
/// </remarks>
internal class RevealTopCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:自分の)?山札の上から1枚を公開(?:し|する)(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "reveal the top card of your deck"
            }
        ];
    }
}
