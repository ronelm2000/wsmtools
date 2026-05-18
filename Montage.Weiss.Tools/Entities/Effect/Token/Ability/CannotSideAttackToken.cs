namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class CannotSideAttackToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカード(?:の正面のキャラ)?はサイドアタックできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var isFacing = match.Value.Contains("の正面のキャラ");
        return
        [
            new CardEffectAbility
            {
                AbilityText = isFacing ? "The character facing this card cannot side attack." : "This card cannot side attack."
            }
        ];
    }
}

internal class CannotPlayBackupDuringBattleToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのバトル中、相手は『助太刀』を手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "During this card's battle, your opponent cannot play \"Backup\" from their hand"
            }
        ];
    }
}
