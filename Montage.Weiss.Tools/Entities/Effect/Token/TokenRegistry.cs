namespace Montage.Weiss.Tools.Entities.Effect.Token;

/// <summary>
/// Generic implementation of IComponentRegistry that manages a collection of CardTextToken instances.
/// Tokens are matched in registration order, with the first match taking precedence.
/// </summary>
/// <typeparam name="E">The type this registry handles (CardEffect, List&lt;CardEffectAbility&gt;, etc.)</typeparam>
/// <remarks>
/// <para><b>Matching Behavior:</b></para>
/// <list type="bullet">
///   <item><description><c>Match</c> - Only returns index-0 matches. Logs warnings for non-zero index matches with skipped text details.</description></item>
///   <item><description><c>GetMatch</c> - Finds all tokens that match the input, warns if multiple match, uses first match</description></item>
///   <item><description><c>TryMatchAtStart</c> - Finds the first token that matches at the start of the input (index 0)</description></item>
///   <item><description><c>TryFindFirstMatch</c> - Finds the token that matches earliest in the input string</description></item>
/// </list>
/// <para><b>Token Registration Order:</b> Tokens should be registered from most specific to most general.
/// More specific patterns should be registered first to ensure they take precedence over general patterns.</para>
/// </remarks>
internal class ComponentRegistry<E> : IComponentRegistry<E>
{
    private readonly List<CardTextToken<E>> _tokens = [];

    public void Register(CardTextToken<E> token) => _tokens.Add(token);

    public IEnumerable<CardTextToken<E>> GetAllTokens() => _tokens;

    public TokenMatchResult<E>? Match(ReadOnlyMemory<char> input)
    {
        TokenMatch? bestAtZero = null;
        Func<ITokenRegistry, E>? bestTranslate = null;
        var nonZeroMatches = new List<(string Token, int Index)>();
        var inputStr = input.ToString();

        foreach (var token in _tokens)
        {
            var match = token.Matcher.Match(inputStr);
            if (!match.Success) continue;

            if (match.Index == 0)
            {
                bestAtZero = new TokenMatch(input, 0, match.Length, token.GetType().Name);
                var slice = input.Slice(match.Index, match.Length);
                bestTranslate = registry => token.Translate(registry, slice);
                break;
            }

            nonZeroMatches.Add((token.GetType().Name, match.Index));
        }

        if (bestAtZero != null)
            return new TokenMatchResult<E>(bestAtZero, bestTranslate!);

        if (nonZeroMatches.Count > 0)
        {
            var minIndex = nonZeroMatches.Min(m => m.Index);
            Log.Warning(
                "No token matched at index 0. {Count} token(s) matched mid-string. Skipped text: '{Skipped}'. Input: '{Input}'. Matches: {Matches}",
                nonZeroMatches.Count,
                inputStr[..minIndex],
                inputStr,
                string.Join(", ", nonZeroMatches.Select(m => $"{m.Token}@{m.Index}")));
        }
        else
        {
            Log.Warning("No token matched at all for input: '{Input}'", inputStr);
        }

        return null;
    }

    public Func<ITokenRegistry, E> GetMatch(ReadOnlyMemory<char> input)
    {
        var match = _tokens.Where(token => token.Matcher.IsMatch(input.Span))
            .ToList();

        if (match.Count > 1 && match[0] is not CardTextToken<CardEffect>)
        {
            Log.Warning("Multiple tokens matched the input: {Input}. This may lead to unpredictable behavior.", input.ToString());
            Log.Warning("Matched tokens: {Tokens}", string.Join(", ", match.Select(t => t.GetType().Name)));
        }

        if (match.Count == 0)
            throw new NotImplementedException($"No token found for input: {input}");
        else
        {
            var enumerator = match[0].Matcher.EnumerateMatches(input.Span);
            foreach (var valueMatch in enumerator)
            {
                var slice = input.Slice(valueMatch.Index, valueMatch.Length);
                return registry => match[0].Translate(registry, slice);
            }
            throw new NotImplementedException($"No token found for input: {input}");
        }
    }

    public bool TryMatchAtStart(string input, out Func<ITokenRegistry, E>? result, out int consumedLength)
    {
        foreach (var token in _tokens)
        {
            var match = token.Matcher.Match(input);
            if (match.Success && match.Index == 0)
            {
                var span = input.AsMemory()[match.Index..match.Length];
                result = registry => token.Translate(registry, span);
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
                var span = input.AsMemory()[match.Index..match.Length];
                result = registry => token.Translate(registry, span);
                matchIndex = match.Index;
                matchLength = match.Length;
            }
        }

        return matchIndex != int.MaxValue;
    }
}
