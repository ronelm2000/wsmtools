namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose 1 opponent front row character and give -power" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の前列のキャラを1枚選び、そのターン中、パワーをー2000。</c> or <c>相手の前列のキャラを1枚選び、そのターン中、パワーを－6000。</c></para>
/// <para><b>Regex:</b> ^相手の前列のキャラを1枚選び、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power reduction value</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your opponent's center stage characters, and that character gets -{power} power until end of turn</c></para>
/// </remarks>
internal class OpponentFrontRowCharacterMinusPowerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のキャラを1枚選び、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手の前列のキャラを1枚選び、そのターン中、パワーをー2000。", "相手の前列のキャラを1枚選び、そのターン中、パワーを－6000。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose 1 of your opponent's center stage characters, and that character gets -{power} power until end of turn"
            }
        ];
    }
}
