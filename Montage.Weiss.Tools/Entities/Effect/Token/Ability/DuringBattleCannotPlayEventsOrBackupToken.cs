namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DuringBattleCannotPlayEventsOrBackupToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのバトル中、相手はイベントと『助太刀』を手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "During this card's battle, your opponent cannot play events or \"Backup\" from their hand."
            }
        ];
    }
}
