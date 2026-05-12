namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class EncoreReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"このカードが舞台から控え室に置かれた時、あなたはコストを払ってよい。そうしたら、このカードがいた枠に【レスト】して置く");

    public override string Translate(ITokenRegistry registry, Match match) =>
        "When this card is put to your waiting room from the stage, you may pay the cost. If you do, return this card to its previous stage position as [REST]";
}
