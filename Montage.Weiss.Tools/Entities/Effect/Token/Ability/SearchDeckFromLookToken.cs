namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches deck-search clauses starting with `山札を見て` (after prefix-stripping of あなたは/自分の).
/// Emits atomic abilities — each returned <see cref="CardEffectAbility"/> represents a single action.
/// The parent (e.g. <see cref="AutoEffectToken"/> via <see cref="AutoEffectToken.JoinAbilityPartsFromSentences"/>)
/// joins them with appropriate connectors (", " then ", and ").
/// Supports dynamic output based on whether "put to hand" and "shuffle" suffixes are present.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>山札を見て《NIKKE》のキャラを1枚まで選んで相手に見せ、手札に加え、その山札をシャッフルする。</c></para>
/// <para><b>Regex:</b> ^山札を見て《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
///   <item><description>Group 2: Pick count (e.g., "1", "X")</description></item>
/// </list>
/// <para><b>Output (atomic abilities, 2–4 items):</b></para>
/// <list type="bullet">
///   <item><description>Base: <c>search your deck for up to 1 &lt;&lt;NIKKE&gt;&gt; character</c></description></item>
///   <item><description>Always: <c>reveal it to your opponent</c></description></item>
///   <item><description>Conditional: <c>put it to your hand</c> (if Japanese contains 手札に加え)</description></item>
///   <item><description>Conditional: <c>shuffle your deck</c> (if Japanese contains シャッフル)</description></item>
/// </list>
/// <para><b>Atomic Ability Pattern:</b> See <c>SearchDeckLevelAndCostToken</c> for the rationale;
/// this token follows the same decomposition pattern.</para>
/// </remarks>
internal class SearchDeckFromLookToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札を見て《(.+?)》のキャラを(.+?)枚まで選んで相手に見せ、(?:.+?)(?:、.+?)*(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value.Replace("Ｘ", "X");
        var fullText = span.ToString();

        var abilities = new List<CardEffectAbility>
        {
            new() { AbilityText = $"search your deck for up to {count} <<{trait}>> character" },
            new() { AbilityText = "reveal it to your opponent" }
        };

        if (fullText.Contains("手札に加え"))
            abilities.Add(new() { AbilityText = "put it to your hand" });
        if (fullText.Contains("シャッフル"))
            abilities.Add(new() { AbilityText = "shuffle your deck" });

        return abilities;
    }
}
