namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DealDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に X ダメージを与える(?:。X はそのカードのレベル＋1 に等しい。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "deal X damage to your opponent. X is equal to that sent card's level +1"
            }
        ];
    }
}

internal class DealVariableDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手に X ダメージを与える。X はそのキャラのソウルに等しい。");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "deal X damage to your opponent. X is equal to that character's soul"
            }
        ];
    }
}
