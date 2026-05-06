using Montage.Weiss.Tools.Entities.Effect;
using Montage.Weiss.Tools.Entities.Effect.Component;

namespace Montage.Weiss.Tools.Impls.Services;

/// <summary>
/// Provides a service for converting effect text into <see cref="WeissSchwarzCardEffect"/>.
/// </summary>
public class WeissSchwarzCardTranslatorService : ITokenRegistry
{
    IComponentRegistry<List<CardEffectAbility>> ITokenRegistry.EffectListRegistry => throw new NotImplementedException();

    IComponentRegistry<List<CardEffectCondition>> ITokenRegistry.ConditionListRegistry => throw new NotImplementedException();

    public string[] MatchLabels(string value)
    {
        throw new NotImplementedException();
    }
}
