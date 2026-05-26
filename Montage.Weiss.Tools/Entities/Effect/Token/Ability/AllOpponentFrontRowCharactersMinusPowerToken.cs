namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class AllOpponentFrontRowCharactersMinusPowerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の前列のキャラすべてに、そのターン中、パワーを[ー－\-](\d+)(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手の前列のキャラすべてに、そのターン中、パワーをー500。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"all of your opponent's characters in their center stage get -{power} power until end of turn"
            }
        ];
    }
}
