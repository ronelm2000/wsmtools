namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class SearchDeckSimpleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたは自分の山札を見て《(?<trait>.+?)》のキャラを(?<count>.+?)枚まで選んで相手に見せ、(?<rest>.+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups["trait"].Value;
        var count = match.Groups["count"].Value.Replace("Ｘ", "X");
        var rest = match.Groups["rest"].Value;

        var additional = rest.Contains("シャッフル") ? ", and shuffle your deck" : "";
        if (rest.Contains("手札に加え"))
            additional = ", put it to your hand" + additional;

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"search your deck for up to {count} <<{trait}>> character, reveal it to your opponent{additional}"
            }
        ];
    }
}
