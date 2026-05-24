namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckSimpleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見て《(?<trait>.+?)》のキャラを(?<count>.+?)枚まで選んで相手に見せ、(?<rest>.+)(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["あなたは自分の山札を見て《★TESTTRAIT★》のキャラを1枚まで選んで相手に見せ、手札に加える。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = registry.MatchNameFragment(match.Groups["trait"].Value);
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        var rest = match.Groups["rest"].Value;

        var additional = "";
        var hasShuffle = rest.Contains("シャッフル");
        if (rest.Contains("手札に加え"))
            additional = ", put it to your hand";

        // Handle trailing choose + power boost: 自分のキャラをN枚選び、そのターン中、パワーを＋N
        var powerBoostMatch = System.Text.RegularExpressions.Regex.Match(rest, @"自分のキャラを(\d+)枚選び、そのターン中、パワーを[＋\+](\d+)");
        var hasPowerBoost = powerBoostMatch.Success;

        if (hasShuffle)
        {
            var shuffleConnector = hasPowerBoost ? ", shuffle your deck" : ", and shuffle your deck";
            additional += shuffleConnector;
        }

        if (hasPowerBoost)
        {
            var charCount = powerBoostMatch.Groups[1].Value;
            var power = powerBoostMatch.Groups[2].Value;
            var sbConnector = hasShuffle && !hasPowerBoost ? "" : ", and";
            additional += $", choose {charCount} of your characters, and that character gets +{power} power until end of turn";
        }

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} <<{trait}>> character, reveal it to your opponent{additional}"
            }
        ];
    }
}
