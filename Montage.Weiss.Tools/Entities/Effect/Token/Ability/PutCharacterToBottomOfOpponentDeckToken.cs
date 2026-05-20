namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "You may put that character at the bottom of opponent's deck" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたはそのキャラを山札の下に置いてよい。</c></para>
/// <para><b>Regex:</b> ^あなたはそのキャラを山札の下に置いてよい (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>You may put that character at the bottom of your opponent's deck</c></para>
/// </remarks>
internal class PutCharacterToBottomOfOpponentDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはそのキャラを山札の下に置いてよい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "You may put that character at the bottom of your opponent's deck"
            }
        ];
    }
}
