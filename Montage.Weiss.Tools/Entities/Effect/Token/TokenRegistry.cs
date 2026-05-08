namespace Montage.Weiss.Tools.Entities.Effect.Token;

internal class ComponentRegistry<E> : IComponentRegistry<E>
{
    private readonly List<CardTextToken<E>> _tokens = [];

    public void Register(CardTextToken<E> token) => _tokens.Add(token);

    public Func<ITokenRegistry, E> GetMatch(string input)
    {
        foreach (var token in _tokens)
        {
            var match = token.Matcher.Match(input);
            if (match.Success)
            {
                return registry => token.Translate(registry, match);
            }
        }
        throw new NotImplementedException($"No token found for input: {input}");
    }

    public bool TryMatchAtStart(string input, out Func<ITokenRegistry, E>? result, out int consumedLength)
    {
        foreach (var token in _tokens)
        {
            var match = token.Matcher.Match(input);
            if (match.Success && match.Index == 0)
            {
                result = registry => token.Translate(registry, match);
                consumedLength = match.Length;
                return true;
            }
        }
        result = null;
        consumedLength = 0;
        return false;
    }

    public bool TryFindFirstMatch(string input, out Func<ITokenRegistry, E>? result, out int matchIndex, out int matchLength)
    {
        result = null;
        matchIndex = int.MaxValue;
        matchLength = 0;

        foreach (var token in _tokens)
        {
            var match = token.Matcher.Match(input);
            if (match.Success && match.Index < matchIndex)
            {
                result = registry => token.Translate(registry, match);
                matchIndex = match.Index;
                matchLength = match.Length;
            }
        }

        return matchIndex != int.MaxValue;
    }
}
