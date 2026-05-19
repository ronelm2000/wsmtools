namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

/// <summary>
/// Matches cost clauses that put a specified number of trait characters from the stage to the waiting room.
/// Unlike <see cref="CostPutTraitCharacterFromStageToWaitingRoomToken"/>, this supports arbitrary counts and omits the "other" qualifier.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>あなたの舞台の《NIKKE》のキャラを3枚控え室に置き</c></para>
/// <para><b>Regex:</b> ^あなたの舞台の《(.+?)》のキャラを(\d+)枚控え室に置き(?:\.|,|、|。)?</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group 1: Trait name (e.g., "NIKKE")</description></item>
///   <item><description>Group 2: Number of characters to put to WR</description></item>
/// </list>
/// <para><b>Output:</b> <c>Put 3 &lt;&lt;NIKKE&gt;&gt; characters in your stage to your waiting room</c></para>
/// </remarks>
internal class CostPutTraitCharactersFromStageToWrToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^あなたの舞台の《(.+?)》のキャラを(\d+)枚控え室に置き(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var trait = match.Groups[1].Value;
        var count = match.Groups[2].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"Put {count} <<{trait}>> characters in your stage to your waiting room"
            }
        ];
    }
}
