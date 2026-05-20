namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put that character to your opponent's memory" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたはそのキャラを思い出にする。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?そのキャラを思い出にする(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>put that character to your opponent's memory</c></para>
/// </remarks>
internal class PutCharacterToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?そのキャラを思い出にする(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "put that character to your opponent's memory"
            }
        ];
    }
}
