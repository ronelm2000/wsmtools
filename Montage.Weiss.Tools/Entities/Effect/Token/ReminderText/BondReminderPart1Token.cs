namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

/// <summary>
/// Matches the first sentence of the bond mechanic reminder text.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードが、手札から舞台またはプレイされて舞台に置かれた時、あなたはコストを払ってよい</c></para>
/// <para><b>Regex:</b> ^このカードが、手札から舞台またはプレイされて舞台に置かれた時、あなたはコストを払ってよい</para>
/// <para><b>Output:</b> <c>When this card is placed on the stage from your hand or played and placed on the stage, you may pay the cost</c></para>
/// </remarks>
internal class BondReminderPart1Token : CardTextToken<string>
{
    public override Regex Matcher => new(@"^このカードが、手札から舞台またはプレイされて舞台に置かれた時、あなたはコストを払ってよい");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) =>
        "When this card is placed on the stage from your hand or played and placed on the stage, you may pay the cost";
}
