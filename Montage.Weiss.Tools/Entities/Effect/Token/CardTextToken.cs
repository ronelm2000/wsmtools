namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal abstract class CardTextToken<E>
{
    public abstract Regex Matcher { get; }
    public abstract E Translate(ITokenRegistry registry, Match match);
}

internal interface ITokenRegistry
{
    IComponentRegistry<List<CardEffectAbility>> EffectListRegistry { get; }
    IComponentRegistry<List<CardEffectCondition>> ConditionListRegistry { get; }
    IComponentRegistry<CardEffect> EffectRegistry { get; }
    IComponentRegistry<string> ReminderTextRegistry { get; }

    string[] MatchLabels(string value);
}

internal interface IComponentRegistry<E>
{
    Func<ITokenRegistry, E> GetMatch(string input);
    bool TryMatchAtStart(string input, out Func<ITokenRegistry, E>? result, out int consumedLength);
    bool TryFindFirstMatch(string input, out Func<ITokenRegistry, E>? result, out int matchIndex, out int matchLength);
}
