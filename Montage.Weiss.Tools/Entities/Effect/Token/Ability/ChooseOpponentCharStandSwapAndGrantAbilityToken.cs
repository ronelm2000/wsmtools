namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class ChooseOpponentCharStandSwapAndGrantAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:あなたは)?相手のキャラを(\d+)枚選び、それぞれを【スタンド】して入れ替え、相手のキャラを(\d+)枚選び、次の相手のターンの終わりまで、次の能力を与える。『(?<abil>.+?)』(?:\.|,|、|。)?");
    public override IEnumerable<string> SampleMatches => ["相手のキャラを2枚選び、それぞれを【スタンド】して入れ替え、相手のキャラを1枚選び、次の相手のターンの終わりまで、次の能力を与える。『【永】 このカードは他の枠に動かせない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var count1 = match.Groups[1].Value;
        var count2 = match.Groups[2].Value;
        var abil = match.Groups["abil"].Value;

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, abil);

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"choose {count1} of your opponent's characters, [STAND] them and switch them, choose {count2} of your opponent's characters, and that character gets the following ability until the end of your opponent's next turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}
