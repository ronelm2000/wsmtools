namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class EncoreReminderPart1Token : CardTextToken<string>
{
    public override Regex Matcher => new(@"^このカードが舞台から控え室に置かれた時、あなたはコストを払ってよい");

    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span) =>
        "When this card is put to your waiting room from the stage, you may pay the cost";
}
