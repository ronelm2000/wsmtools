namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose a trait character and power boost" clauses with plural-aware subject-verb agreement.
/// Supports an optional "other" qualifier, an optional <c>あなたは</c> prefix, singular/plural pronoun selection,
/// and full-width <c>Ｘ</c> for variable power values.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたは他の自分の《NIKKE》のキャラを1枚選び、そのターン中、パワーを＋2000。</c></para>
/// <para><b>Regex:</b> ^(?:あなたは)?(?:他の)?自分の《(.+?)》のキャラを(\d+)枚選び、そのターン中、パワーを[＋\+]([Ｘ\d]+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
///   <item><description>Group 2: Character count (e.g., "1")</description></item>
///   <item><description>Group 3: Power boost value (e.g., "2000" or "Ｘ")</description></item>
/// </list>
/// <para><b>Output:</b> <c>choose 1 of your other &lt;&lt;NIKKE&gt;&gt; characters, and it gets +2000 power until end of turn</c></para>
/// </remarks>
internal class ChooseTraitCharacterAndPowerBoostToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?(?:他の)?自分の《(.+?)》のキャラを(\d+)枚選び、そのターン中、パワーを[＋\+]([Ｘ\d]+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var count = int.Parse(match.Groups[2].Value);
        var power = match.Groups[3].Value.Replace('Ｘ', 'X');
        var hasOther = match.Value.Contains("他の", StringComparison.Ordinal);
        var otherText = hasOther ? "other " : "";
        var verb = count == 1 ? "it gets" : "those characters get";
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"choose {count} of your {otherText}<<{trait}>> characters, and {verb} +{power} power until end of turn"
            }
        ];
    }
}
