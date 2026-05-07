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
}
