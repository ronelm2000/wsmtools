namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentLevelHigherThenExchangeToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手のレベル(\d+)以上のキャラを1枚選んでよい。そうしたら、相手は自分の控え室のレベル[XＸ]以下のキャラを1枚選び、入れ替える(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手のレベル1以上のキャラを1枚選んでよい。そうしたら、相手は自分の控え室のレベルＸ以下のキャラを1枚選び、入れ替える"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        if (!match.Success)
            return [];
        var threshold = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"you may choose 1 of your opponent's level {threshold} or higher characters. If you do, your opponent chooses 1 level X or lower character in their waiting room, and switches them"
            }
        ];
    }
}
