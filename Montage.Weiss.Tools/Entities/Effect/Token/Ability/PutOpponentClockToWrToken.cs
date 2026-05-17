namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Put opponent clock top card to WR" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手のクロックの上から 1 枚を、控え室に置いてよい。</c></para>
/// <para><b>Regex:</b> ^相手のクロックの上から 1 枚を、控え室に置いてよい (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>You may put the top card of your opponent's clock to their waiting room</c></para>
/// </remarks>
internal class PutOpponentClockToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のクロックの上から1枚を、控え室に置いてよい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "You may put the top card of your opponent's clock to their waiting room"
            }
        ];
    }
}
