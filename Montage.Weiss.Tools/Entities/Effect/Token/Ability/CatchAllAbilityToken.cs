namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Catch-all token that matches any remaining ability text that no specific ability token recognized.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> Any text that reached the ability loop's break point after all
/// direct-match and prefix-skip attempts failed.</para>
/// <para><b>Regex:</c> ^.+</para>
/// <para><b>Captures:</b> None (the entire match is the unrecognized text).</para>
/// <para><b>Output:</b> A <see cref="CardEffectAbility"/> with <see cref="CardEffectAbility.IsUnmatched"/> set to <c>true</c>.</para>
/// <para><b>Registration:</b> NOT registered in <see cref="IComponentRegistry{T}"/> — doing so would
/// cause false positives by matching before the prefix-skip pipeline runs in <see cref="MultiClauseEffectParser.ParseSentence"/>.
/// Instead, invoked explicitly at the ability loop's break point in <see cref="MultiClauseEffectParser"/>.</para>
/// <para><b>Purpose:</b> Development aid — logs a warning and produces a sentinel ability that causes a
/// <see cref="NotImplementedException"/> after the full <see cref="CardEffectTree"/> is built,
/// showing developers which ability text needs a new token.</para>
/// </remarks>
internal class CatchAllAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<CatchAllAbilityToken>();

    public override Regex Matcher => new(@"^.+");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var text = span.ToString();
        Log.Warning("CatchAllAbilityToken: unrecognized ability '{Text}'", text);
        return
        [
            new CardEffectAbility
            {
                AbilityText = text,
                IsUnmatched = true
            }
        ];
    }
}
