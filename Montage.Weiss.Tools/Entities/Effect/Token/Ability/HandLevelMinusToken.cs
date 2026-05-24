namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "get -N level while in your hand" clauses.
/// Uses <c>get</c> (base verb) for named card subjects (<c>your "CardName" get -N level</c>)
/// and <c>gets</c> (third-person singular) for <c>this card</c> subject.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの手札のこのカードのレベルを－1</c> or <c>あなたの手札の「カード名」のレベルを－1</c></para>
/// <para><b>Regex:</b> ^あなたの手札の(?:(?:このカード)|「(?&lt;name&gt;.+?)」)のレベルを－(\d+)(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "name": Optional card name (present for named cards, absent for "this card")</description></item>
///   <item><description>Group 1: Level decrease value (e.g., "1")</description></item>
/// </list>
/// <para><b>Output (named card):</b> <c>your "CardName" get -N level while in your hand</c></para>
/// <para><b>Output (this card):</b> <c>this card gets -N level while in your hand</c></para>
/// </remarks>
internal class HandLevelMinusToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの手札の(?:(?:このカード)|「(?<name>.+?)」)のレベルを－(\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = match.Groups["name"].Success ? registry.MatchNameFragment(match.Groups["name"].Value) : null;
        var level = match.Groups[1].Value;
        
        // Clean up nested quotes for proper English formatting
        if (!string.IsNullOrEmpty(name))
        {
            // Remove trailing double quote if present
            name = name.TrimEnd('"');
            // Replace triple quotes with double quotes for proper formatting
            name = name.Replace("\"\"\"", "\"\"");
        }
        
        var abilityText = !string.IsNullOrEmpty(name)
            ? $"your \"{name}\" get -{level} level while in your hand"
            : $"this card gets -{level} level while in your hand";
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}
