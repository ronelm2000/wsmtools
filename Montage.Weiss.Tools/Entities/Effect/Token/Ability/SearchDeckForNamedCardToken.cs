namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckForNamedCardToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^山札を見て「(?<name>.+?)」を(?<count>.+?)枚まで選んで相手に見せ、(?<rest>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var name = registry.MatchNameFragment(match.Groups["name"].Value);
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        var rest = match.Groups["rest"].Value;

        var additional = "";
        if (rest.Contains("手札に加え"))
            additional = ", put it to your hand";
        if (rest.Contains("シャッフル"))
            additional += ", and shuffle your deck";

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} \"{name}\", reveal it to your opponent{additional}"
            }
        ];
    }
}
