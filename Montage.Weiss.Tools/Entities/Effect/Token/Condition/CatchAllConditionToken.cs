namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

/// <summary>
/// Catch-all token that matches any condition-like text ending with a condition marker
/// (なら, 時, たび, 限り, 場合) that no specific condition token recognized.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> Any text ending with <c>なら</c>, <c>時</c>, <c>たび</c>, <c>限り</c>, or <c>場合</c> followed by <c>、</c> or end of string.</para>
/// <para><b>Regex:</b> ^(?:(?:『[^』]*』)|[^『])+?(?&lt;marker&gt;なら|時|たび|限り|場合)(?:、|$)</para>
/// <para><b>Note:</b> Skips over <c>『...』</c> quoted blocks to avoid matching condition markers inside sub-ability text.</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>marker: The condition-ending marker used to infer <see cref="ConditionType"/></description></item>
/// </list>
/// <para><b>Output:</b> A <see cref="CardEffectCondition"/> with <see cref="CardEffectCondition.IsUnmatched"/> set to <c>true</c>.
/// The <see cref="ConditionType"/> is inferred from the ending marker: <c>時</c>/<c>たび</c> → <see cref="ConditionType.When"/>,
/// <c>限り</c> → <see cref="ConditionType.During"/>, <c>なら</c>/<c>場合</c> → <see cref="ConditionType.If"/>.</para>
/// <para><b>Registration:</b> Must be registered LAST in the condition registry so it only fires when no specific token matches.</para>
/// <para><b>Purpose:</b> Development aid — logs a warning and produces a sentinel condition that causes a
/// <see cref="NotImplementedException"/> after the full <see cref="CardEffectTree"/> is built,
/// showing developers which condition text needs a new token.</para>
/// </remarks>
internal class CatchAllConditionToken : CardTextToken<List<CardEffectCondition>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CatchAllConditionToken>();

    public override Regex Matcher => new(@"^(?:(?:『[^』]*』)|[^『])+?(?<marker>なら|時|たび|限り|場合)(?:、|$)");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var text = span.ToString();
        var match = Matcher.Match(text);
        var marker = match.Groups["marker"].Value;
        var cleanedText = text.TrimEnd('、');

        var type = marker switch
        {
            "時" or "たび" => ConditionType.When,
            "限り"         => ConditionType.During,
            _              => ConditionType.If
        };

        Log.Warning("CatchAllConditionToken: unrecognized {Type} condition '{Text}'", type, cleanedText);

        return
        [
            new CardEffectCondition
            {
                Type = type,
                ConditionText = cleanedText,
                IsUnmatched = true
            }
        ];
    }
}
