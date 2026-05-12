namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class DealDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"相手にＸダメージを与える(?:。Ｘはそのカードのレベル＋1に等しい。)?");

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
    public override Regex Matcher => new(@"相手にＸダメージを与える。Ｘはそのキャラのソウルに等しい。");

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
