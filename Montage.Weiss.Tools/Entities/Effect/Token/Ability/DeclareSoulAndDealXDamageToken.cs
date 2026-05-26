namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "declare a number ≤ soul, deal X damage, -X soul, get following ability" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのソウル以下の好きな数を宣言し、相手にＸダメージを与え、そのターン中、このカードのソウルを－Ｘし、このカードは次の能力を得る。『…』</c></para>
/// <para><b>Regex:</b> ^このカードのソウル以下の好きな数を宣言し、相手に[XＸ]ダメージを与え、そのターン中、このカードのソウルを[ー－\-][XＸ]し、このカードは次の能力を得る。『(?&lt;nested&gt;.+)』(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>declare a number less than or equal to this card's soul, deal X damage to your opponent, this card gets -X soul until end of turn, and gets the following ability. "{nestedEnglish}"</c></para>
/// <para><b>Note:</b> The post-condition "X is equal to the declared number" is handled separately by <see cref="Condition.XEqualsConditionToken"/>.</para>
/// <para><b>Scope Expansion:</b> To support variations, add alternative patterns for:
/// - Different phrasing (ソウル未満 instead of ソウル以下)
/// - Different variable names (Ｙ instead of Ｘ)
/// - Different nested ability quote styles</para>
/// </remarks>
internal class DeclareSoulAndDealXDamageToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカードのソウル以下の好きな数を宣言し、相手に[XＸ]ダメージを与え、そのターン中、このカードのソウルを[ー－\-][XＸ]し、このカードは次の能力を得る。『(?<nested>.+)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["このカードのソウル以下の好きな数を宣言し、相手にＸダメージを与え、そのターン中、このカードのソウルを－Ｘし、このカードは次の能力を得る。『【自】［あなたのCX置場のCXを1枚控え室に置く］ あなたのアンコールステップの始めに、あなたはコストを払ってよい。そうしたら、あなたは自分の控え室のカードすべてを、山札に戻し、その山札をシャッフルする。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
        var nestedEnglish = nestedEffect.EffectText;
        if (!nestedEnglish.EndsWith('.') && !nestedEnglish.EndsWith('"'))
            nestedEnglish += ".";
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"declare a number less than or equal to this card's soul, deal X damage to your opponent, this card gets -X soul until end of turn, and gets the following ability. \"{nestedEnglish}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
