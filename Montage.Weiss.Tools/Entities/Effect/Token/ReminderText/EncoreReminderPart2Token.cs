namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class EncoreReminderPart2Token : CardTextToken<string>
{
    public override Regex Matcher => new(@"そうしたら、このカードがいた枠に【レスト】して置く");

    public override string Translate(ITokenRegistry registry, Match match) =>
        "If you do, return this card to its previous stage position as [REST]";
}
