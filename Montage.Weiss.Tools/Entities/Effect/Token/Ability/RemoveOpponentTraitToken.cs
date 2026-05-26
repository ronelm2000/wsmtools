namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "choose 1 trait on opponent stage character, all opponent characters lose that trait" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>相手の舞台にいるキャラの特徴を1つ選び、相手のキャラすべては、そのターン中、その特徴をすべて失う</c></para>
/// <para><b>Regex:</b> ^相手の舞台にいるキャラの特徴を1つ選び、相手のキャラすべては、そのターン中、その特徴をすべて失う(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b> None (fixed pattern)</para>
/// <para><b>Output:</b> <c>choose 1 trait on a character in your opponent's stage, and all of your opponent's characters lose that trait until end of turn</c></para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different target location (前列, 後列 instead of 舞台)
/// - Different target count (複数の特徴 instead of 1つ)
/// - Different duration (永続的 instead of そのターン中)</para>
/// </remarks>
internal class RemoveOpponentTraitToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^相手の舞台にいるキャラの特徴を1つ選び、相手のキャラすべては、そのターン中、その特徴をすべて失う(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手の舞台にいるキャラの特徴を1つ選び、相手のキャラすべては、そのターン中、その特徴をすべて失う。"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "choose 1 trait on a character in your opponent's stage, and all of your opponent's characters lose that trait until end of turn"
            }
        ];
    }
}
