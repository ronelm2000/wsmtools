namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class IfYouDoToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^そうしたら、");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "If you do",
                Prefix = AbilityPrefix.IfYouDo
            }
        ];
    }
}
