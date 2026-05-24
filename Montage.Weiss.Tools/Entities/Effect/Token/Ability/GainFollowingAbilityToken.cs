namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostWithDurationToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly ILogger Log = Serilog.Log.ForContext<PowerBoostWithDurationToken>();

    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?<power>\d+)(?:\.|,|、|。)?");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        Log.Debug("PowerBoostWithDurationToken: matched input='{Input}', power={Power}, duration='{Duration}'",
            span.ToString(), power, durationText);

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power{durationText}"
            }
        ];
    }
}

/// <summary>
/// Matches "power boost and gain following ability" clauses.
/// Emits Oxford comma before "and the following ability" and ensures the nested English text ends with a period.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>このカードのパワーを＋4500し、このカードは次の能力を得る。『…』</c></para>
/// <para><b>Regex:</b> ^(?:(?&lt;duration&gt;そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?&lt;power&gt;\d+)し、このカードは次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>Group "duration": Optional duration prefix</description></item>
///   <item><description>Group "power": Power boost amount (e.g., "4500")</description></item>
///   <item><description>Group "nested": Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets +{power} power, and the following ability{duration}. "{nestedEnglish}"</c></para>
/// </remarks>
internal class PowerBoostWithFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", " until end of turn" },
        { "このターン中", " until end of turn" },
        { "次の相手のターンの終わりまで", " until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", " until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを[＋\+](?<power>\d+)し、このカードは次の能力を得る。『(?<nested>.+)』");

    public override IEnumerable<string> SampleMatches => ["このカードのパワーを＋4500し、このカードは次の能力を得る。『【自】 このカードがアタックした時、自分の山札の上から1枚をストック置場に置いてよい。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;
        var nestedJapanese = match.Groups["nested"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = dur;
        }

        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
        var nestedEnglish = nestedEffect.EffectText;

        if (!nestedEnglish.EndsWith('.') && !nestedEnglish.EndsWith('"'))
            nestedEnglish += ".";
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets +{power} power, and the following ability{durationText}. \"{nestedEnglish}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }

    internal static CardEffect TranslateNested(ITokenRegistry registry, string japanese)
    {
        var matchResult = registry.EffectRegistry.Match(japanese.Trim().AsMemory());
        if (matchResult != null)
        {
            return matchResult.Translate(registry);
        }

        return new EventCardEffect
        {
            Labels = [],
            EffectText = japanese,
            AbilityText = japanese,
            Abilities = [new UnmatchedAbility
            {
                AbilityText = japanese,
                IsUnmatched = true,
                Suggestions = ["unmatched nested ability text"]
            }]
        };
    }
}

internal class PowerBoostWithFollowingAbilitiesToken : CardTextToken<List<CardEffectAbility>>
{
    private static readonly Dictionary<string, string> DurationMap = new()
    {
        { "そのターン中", "until end of turn" },
        { "このターン中", "until end of turn" },
        { "次の相手のターンの終わりまで", "until the end of your opponent's next turn" },
        { "次の相手のターンの終了時まで", "until the end of your opponent's next turn" },
    };

    public override Regex Matcher => new(@"^(?:(?<duration>そのターン中|このターン中|次の相手のターンの終わりまで|次の相手のターンの終了時まで)、)?このカードのパワーを＋(?<power>\d+)し、このカードは次の2つの能力を得る。『(?<nested1>.+)』『(?<nested2>.+)』");

    public override IEnumerable<string> SampleMatches => ["このカードのパワーを＋3000し、このカードは次の2つの能力を得る。『【永】 このカードは相手の効果に選ばれない。』『【永】 大活躍』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var durationGroup = match.Groups["duration"];
        var power = match.Groups["power"].Value;
        var nestedJapanese1 = match.Groups["nested1"].Value;
        var nestedJapanese2 = match.Groups["nested2"].Value;

        var durationText = "";
        if (durationGroup.Success && DurationMap.TryGetValue(durationGroup.Value, out var dur))
        {
            durationText = $" {dur}";
        }

        var nestedEffect1 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese1);
        var nestedEffect2 = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese2);
        var nestedEnglish1 = nestedEffect1.EffectText;
        if (!nestedEnglish1.EndsWith('.') && !nestedEnglish1.EndsWith('"') && !nestedEnglish1.EndsWith(']') && nestedEnglish1.Contains(' '))
            nestedEnglish1 += ".";

        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and the following abilities{durationText}. \"{nestedEnglish1}\" \"{nestedEffect2.EffectText}\"",
                NestedEffect = nestedEffect1,
                IsUnmatched = nestedEffect1.Abilities.Any(a => a.IsUnmatched) || nestedEffect2.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}

internal class GainFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^し、このカードが次の能力を得る(?:。『(.+)』)?(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["し、このカードが次の能力を得る。『【自】 このカードがアタックした時、相手のキャラを1枚選び、思い出にする。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Success ? match.Groups[1].Value : null;
        var abilityText = "get the following ability";
        if (nestedJapanese != null)
        {
            var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
            abilityText += $". \"{nestedEffect.EffectText}\"";
            return
            [
                new NestedCardEffectAbility
                {
                    AbilityText = abilityText,
                    NestedEffect = nestedEffect,
                    IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
                }
            ];
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}

internal class GainFollowingAbilityTokenWithParticleWa : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^し、このカードは次の能力を得る(?:。『(.+)』)?(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["し、このカードは次の能力を得る。『【永】 このカードは相手の効果に選ばれない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups[1].Success ? match.Groups[1].Value : null;
        var abilityText = "get the following ability";
        if (nestedJapanese != null)
        {
            var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
            abilityText += $". \"{nestedEffect.EffectText}\"";
            return
            [
                new NestedCardEffectAbility
                {
                    AbilityText = abilityText,
                    NestedEffect = nestedEffect,
                    IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
                }
            ];
        }
        return
        [
            new CardEffectAbility
            {
                AbilityText = abilityText
            }
        ];
    }
}

/// <summary>
/// Matches standalone "<c>次の能力を得る。『...』</c>" clauses (no leading <c>し、</c>).
/// Used after prefixes like <c>そのターン中、このカードは</c> have been stripped.
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>次の能力を得る。『【永】 あなたの[[shot.gif]]の効果で与えるダメージを＋1。』</c></para>
/// <para><b>Regex:</b> ^次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>get the following ability. "[CONT] ..."</c></para>
/// </remarks>
internal class GainStandaloneFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^次の能力を得る。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["次の能力を得る。『【永】 あなたの[[shot.gif]]の効果で与えるダメージを＋1。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"get the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}

/// <summary>
/// Matches "<c>(その後、)?あなたのキャラすべてに...次の能力を与える。『...』</c>" clauses.
/// Used for granting abilities to all characters, optionally after "その後、".
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>その後、あなたのキャラすべてに、そのターン中、次の能力を与える。『【自】 このカードがアタックした時、...』</c></para>
/// <para><b>Regex:</b> ^(?:その後、)?あなたのキャラすべてに、そのターン中、次の能力を与える。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>all of your characters get the following ability until end of turn. "[AUTO] ..."</c></para>
/// </remarks>
internal class AfterThatAllCharactersGetAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:その後、)?あなたのキャラすべてに、そのターン中、次の能力を与える。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["あなたのキャラすべてに、そのターン中、次の能力を与える。『【自】 このカードがアタックした時、自分の山札の上から1枚をストック置場に置く。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = $"all of your characters get the following ability until end of turn. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}

/// <summary>
/// Matches "<c>(そのターン中、)?(?:このカードは)?次の能力を得る。『...』</c>" clauses.
/// Variant that captures an optional duration prefix before "次の能力を得る".
/// </summary>
/// <remarks>
/// <para><b>Expected Input:</b> <c>そのターン中、このカードは次の能力を得る。『【自】 ...』</c></para>
/// <para><b>Regex:</b> ^(?:そのターン中、)?(?:このカードは)?次の能力を得る。『(?&lt;nested&gt;.+)』</para>
/// <para><b>Captures:</b></para>
/// <list type="bullet">
///   <item><description>nested: Inner quoted ability text</description></item>
/// </list>
/// <para><b>Output:</b> <c>this card gets the following ability until end of turn. "[AUTO] ..."</c> (with duration) / <c>get the following ability. "[AUTO] ..."</c> (without duration)</para>
/// </remarks>
internal class GainFollowingAbilityWithDurationToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^(?:そのターン中、)?(?:このカードは)?次の能力を得る。『(?<nested>.+?)』(?:\.|,|、|。)?");

    public override IEnumerable<string> SampleMatches => ["そのターン中、このカードは次の能力を得る。『【永】 このカードは【リバース】しない。』"];

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, ReadOnlyMemory<char> span)
    {
        var match = Matcher.Match(span.ToString());
        var nestedJapanese = match.Groups["nested"].Value;
        var nestedEffect = PowerBoostWithFollowingAbilityToken.TranslateNested(registry, nestedJapanese);
        var hasDuration = match.Value.Contains("そのターン中");
        return
        [
            new NestedCardEffectAbility
            {
                AbilityText = hasDuration
                    ? $"this card gets the following ability until end of turn. \"{nestedEffect.EffectText}\""
                    : $"get the following ability. \"{nestedEffect.EffectText}\"",
                NestedEffect = nestedEffect,
                IsUnmatched = nestedEffect.Abilities.Any(a => a.IsUnmatched)
            }
        ];
    }
}

