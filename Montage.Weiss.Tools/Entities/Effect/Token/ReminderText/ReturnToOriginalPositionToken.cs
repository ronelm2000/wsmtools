namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class ReturnToOriginalPositionToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"公開したカードは元に戻す");

    public override string Translate(ITokenRegistry registry, Match match) => "Put it on its original position";
}
