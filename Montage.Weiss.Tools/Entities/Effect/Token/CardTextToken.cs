namespace Montage.Weiss.Tools.Entities.Effect.Component;

internal abstract class CardTextToken<E>
{
    public abstract Regex Matcher { get; }
    public abstract E Translate(ITokenRegistry registry, Match match);
}

internal interface ITokenRegistry
{
    IComponentRegistry<List<CardEffectAbility>> EffectListRegistry { get; }
    IComponentRegistry<List<CardEffectCondition>> ConditionListRegistry { get; }

    string[] MatchLabels(string value);
}

internal interface IComponentRegistry<E>
{
    Func<ITokenRegistry, E> GetMatch(string input);
}