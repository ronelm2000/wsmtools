namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "put that character on the top of your opponent's deck" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたはそのキャラを山札の上に置いてよい。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?そのキャラを山札の上に置いてよい(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>you may put that character on the top of your opponent's deck</c></para>
/// </remarks>
internal class PutCharacterToTopOfDeckToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?そのキャラを山札の上に置いてよい(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "you may put that character on the top of your opponent's deck"
}
        ];
    }
}
