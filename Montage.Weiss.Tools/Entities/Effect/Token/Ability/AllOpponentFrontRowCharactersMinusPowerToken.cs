namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "all opponent front row characters get -power" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の前列のキャラすべてに、そのターン中、パワーをー500。</c></para>
/// <para><b>Regex:</b> ^相手の前列のキャラすべてに、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Power reduction value</description></item>
/// </list>
/// <para><b>Output:</b> <c>all of your opponent's center stage characters get -{power} power until end of turn</c></para>
/// </remarks>
internal class AllOpponentFrontRowCharactersMinusPowerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のキャラすべてに、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手の前列のキャラすべてに、そのターン中、パワーをー500。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your opponent's center stage characters get -{power} power until end of turn"
            }
        ];
    }
}
