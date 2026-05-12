namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CannotPlayEventsOrBackupFromHandToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたはイベントと『助太刀』を手札からプレイできない。$");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "You cannot play events or \"Backup\" in your hand."
            }
        ];
    }
}
