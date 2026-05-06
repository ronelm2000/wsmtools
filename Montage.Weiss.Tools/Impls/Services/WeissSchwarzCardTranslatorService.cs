using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Token;
using Montage.Weiss.Tools.Entities.Effect.Token.Ability;
using Montage.Weiss.Tools.Entities.Effect.Token.Condition;

namespace Montage.Weiss.Tools.Impls.Services;

/// <summary>
/// Provides a service for converting effect text into <see cref="CardEffectTree"/>.
/// </summary>
public class WeissSchwarzCardTranslatorService : ITokenRegistry
{
    private readonly ComponentRegistry<List<CardEffectAbility>> _effectListRegistry = new();
    private readonly ComponentRegistry<List<CardEffectCondition>> _conditionListRegistry = new();
    private readonly ComponentRegistry<CardEffect> _effectRegistry = new();

    public WeissSchwarzCardTranslatorService()
    {
        RegisterDefaultTokens();
    }

    private void RegisterDefaultTokens()
    {
        // Register condition tokens
        _conditionListRegistry.Register(new HandSizeConditionToken());
        _conditionListRegistry.Register(new TurnAndTraitCharacterCountConditionToken());

        // Register ability tokens
        _effectListRegistry.Register(new PowerBoostToken());

        // Register effect type tokens
        _effectRegistry.Register(new ContEffectToken());
    }

    IComponentRegistry<List<CardEffectAbility>> ITokenRegistry.EffectListRegistry => _effectListRegistry;

    IComponentRegistry<List<CardEffectCondition>> ITokenRegistry.ConditionListRegistry => _conditionListRegistry;

    IComponentRegistry<CardEffect> ITokenRegistry.EffectRegistry => _effectRegistry;

    public string[] MatchLabels(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return value.Split('】', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.TrimStart('【'))
            .ToArray();
    }

    public CardEffectTree TranslateEffect(string japaneseEffectText)
    {
        var effect = _effectRegistry.GetMatch(japaneseEffectText)(this);
        return new CardEffectTree
        {
            Effects = [effect]
        };
    }
}
