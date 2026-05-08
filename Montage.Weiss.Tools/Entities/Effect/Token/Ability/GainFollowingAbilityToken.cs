namespace Montage.Weiss.Tools.Entities.Effect.Token.Ability;

internal class PowerBoostWithFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカードのパワーを＋(\d+)し、このカードは次の能力を得る。『(.+)』");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        var nestedJapanese = match.Groups[2].Value;

        string nestedEnglish;
        try
        {
            var nestedEffect = registry.EffectRegistry.GetMatch(nestedJapanese)(registry);
            nestedEnglish = nestedEffect.EffectText;
        }
        catch (NotImplementedException)
        {
            nestedEnglish = nestedJapanese;
        }

        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power and the following ability. \"{nestedEnglish}\""
            }
        ];
    }
}

internal class GainFollowingAbilityToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"し、このカードが次の能力を得る");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}

internal class GainFollowingAbilityTokenV2 : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"し、このカードが次の能力を得る");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}

internal class GainFollowingAbilityTokenV3 : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"し、このカードは次の能力を得る");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}

internal class PowerAndGainCombinedJapaneseToken : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"^このカード next ability obtain");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        return
        [
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}

internal class PowerAndGainCombinedJapaneseTokenWithPower : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカード next ability obtain\+(\d+) power");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power"
            },
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}

internal class PowerAndGainCombinedJapaneseTokenWithPowerV2 : CardTextToken<List<CardEffectAbility>>
{
    public override Regex Matcher => new(@"このカード next ability obtain\+(\d+) power");

    public override List<CardEffectAbility> Translate(ITokenRegistry registry, Match match)
    {
        var power = match.Groups[1].Value;
        return
        [
            new CardEffectAbility
            {
                AbilityText = $"this card gets +{power} power"
            },
            new CardEffectAbility
            {
                AbilityText = "get the following ability"
            }
        ];
    }
}