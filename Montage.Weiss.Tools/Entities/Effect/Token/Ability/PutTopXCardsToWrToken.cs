namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Put top X cards to WR" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札の上から X 枚を、控え室に置いてよい。X はあなたの《NIKKE》のキャラの枚数に等しい。</c></para>
/// <para><b>Regex:</b> ^自分の山札の上から X 枚を、控え室に置いてよい。X はあなたの《(.+?)》のキャラの枚数に等しい (?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
/// </list>
/// <para><b>Output:</b> <c>You may put the top X cards of your deck to your waiting room. X is equal to the number of &lt;&lt;NIKKE&gt;&gt; characters you have</c></para>
/// </remarks>
internal class PutTopXCardsToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の山札の上から[XＸ]枚を、控え室に置いてよい。?[XＸ]はあなたの《(.+?)》のキャラの枚数に等しい");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (match.Success)
        {
            var trait = match.Groups[1].Value;
            return
            [
                new CardEffectAbility
                {
                    AbilityText = $"You may put the top X cards of your deck to your waiting room. X is equal to the number of <<{trait}>> characters you have"
                }
            ];
        }
        return [];
    }
}
