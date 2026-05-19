namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches combined "play without color condition, otherwise cannot play" clauses.
/// When the condition has already been consumed by a condition token, use <see cref="PlayWithoutColorConditionToken"/> instead.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは、色条件を満たさずに手札からプレイできる。そうでないなら、手札からプレイできない</c></para>
/// <para><b>Regex:</b> ^このカードは、色条件を満たさずに手札からプレイできる。そうでないなら、手札からプレイできない(?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>this card can be played in your hand without fulfilling color requirements. Otherwise, this card cannot be played in your hand</c></para>
/// </remarks>
internal class ColorConditionIgnorePlayToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは、色条件を満たさずに手札からプレイできる。そうでないなら、手札からプレイできない(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card can be played in your hand without fulfilling color requirements. Otherwise, this card cannot be played in your hand"
            }
        ];
    }
}

/// <summary>
/// Matches "can play without color condition" clauses (standalone, without the "otherwise" fallback).
/// Used when the condition portion has already been consumed by a condition token.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードは、色条件を満たさずに手札からプレイできる</c></para>
/// <para><b>Regex:</b> ^このカードは、色条件を満たさずに手札からプレイできる</para>
/// <para><b>Output:</b> <c>this card can be played in your hand without fulfilling color requirements</c></para>
/// </remarks>
internal class PlayWithoutColorConditionToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードは、色条件を満たさずに手札からプレイできる(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "this card can be played in your hand without fulfilling color requirements"
            }
        ];
    }
}
