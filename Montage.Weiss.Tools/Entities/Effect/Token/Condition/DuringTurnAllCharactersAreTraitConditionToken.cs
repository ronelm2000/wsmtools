namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringTurnAllCharactersAreTraitConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^あなたのターン中、あなたのキャラすべてが《(?<trait>.+?)》なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups["trait"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.During,ConditionText = $"During your turn, if all of your characters are <<{trait}>>"
            }
        ];
    }
}
