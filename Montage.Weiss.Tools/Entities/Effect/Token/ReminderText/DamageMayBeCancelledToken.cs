namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class DamageMayBeCancelledToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"ダメージキャンセルは発生する");

    public override string Translate(ITokenRegistry registry, Match match) => "Damage may be cancelled";
}
