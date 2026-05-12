namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class ReturnToOriginalPositionOtherwiseToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^そうでないなら元に戻す");

    public override string Translate(ITokenRegistry registry, Match match) => "Otherwise, return it to its original position";
}
