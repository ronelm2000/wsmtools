namespace Montage.Weiss.Tools.Entities.Effect.Token.Condition;

internal class DuringOpponentTurnAllCharactersAreTraitConditionToken : CardTextToken<List<CardEffectCondition>>
{
    public override Regex Matcher => new(@"^相手のターン中、あなたのキャラすべてが《(?<trait>.+?)》なら");

    public override List<CardEffectCondition> Translate(ITokenRegistry registry, Match match)
    {
        var trait = match.Groups["trait"].Value;
        return
        [
            new CardEffectCondition
            {
                
            Type = ConditionType.During,ConditionText = $"During your opponent's turn, if all of your characters are <<{trait}>>"
            }
        ];
    }
}
