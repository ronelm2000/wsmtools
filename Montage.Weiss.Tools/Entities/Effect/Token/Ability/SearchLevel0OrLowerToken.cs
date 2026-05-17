namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches "Search level 0 or lower character" clauses.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>自分の山札を見てレベル 0 以下のキャラを 1 枚まで選び</c></para>
/// <para><b>Regex:</b> ^自分の山札を見てレベル 0 以下のキャラを 1 枚まで選び (?:\.|,|、|。)?</para>
/// <para><b>Output:</b> <c>Search your deck for up to 1 level 0 or lower character</c></para>
/// </remarks>
internal class SearchLevel0OrLowerToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^自分の山札を見てレベル0以下のキャラを1枚まで選び");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "Search your deck for up to 1 level 0 or lower character"
            }
        ];
    }
}
