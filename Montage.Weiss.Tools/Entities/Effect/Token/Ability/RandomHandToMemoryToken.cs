namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class RandomHandToMemoryToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の手札をランダムに1枚選んで公開し、思い出にし、次の相手のターンの終わりに、相手はそのカードを手札に戻す(?:\.|,|、|。)?");
    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 card from your opponent's hand at random, reveal it, put it into memory, and your opponent returns it to their hand at the end of your opponent's next turn"
            }
        ];
    }
}
