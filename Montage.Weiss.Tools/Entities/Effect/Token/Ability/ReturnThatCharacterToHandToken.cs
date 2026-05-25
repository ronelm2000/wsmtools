namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "return that character to hand" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのキャラを手札に戻す。</c></para>
/// <para><b>Regex:</b> ^そのキャラを手札に戻す(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>return that character to your hand</c></para>
/// </remarks>
internal class ReturnThatCharacterToHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そのキャラを手札に戻す(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["そのキャラを手札に戻す。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "return that character to your hand"
            }
        ];
    }
}
