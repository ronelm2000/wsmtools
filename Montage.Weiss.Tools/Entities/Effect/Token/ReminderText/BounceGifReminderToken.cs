namespace Montage.Weiss.Tools.Entities.Effect.Token.ReminderText;

internal class BounceGifReminderToken : CardTextToken<string>
{
    public override Regex Matcher => new(@"^\[\[bounce\.gif\]\][：:]\s*このカードがトリガーした時、あなたは相手のキャラを1枚選び、手札に戻してよい\)?");
    public override string Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return "[BOUNCE]: When this card triggers, you may choose 1 of your opponent's characters, and return it to your hand";
    }
}
