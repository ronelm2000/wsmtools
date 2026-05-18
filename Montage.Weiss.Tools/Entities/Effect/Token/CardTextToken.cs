namespace Montage.Weiss.Tools.Entities.Effect.Token;

public record TokenMatch(
    ReadOnlyMemory<char> Input,
    int Index,
    int Length,
    string Token);

public record TokenMatchResult<E>(
    TokenMatch Match,
    Func<ITokenRegistry, E> Translate);

/// <summary>
/// Base class for all Japanese card text parsing tokens.
/// Each token defines a regex pattern that matches specific Japanese text clauses
/// and a translation method that converts matched text into structured representations.
/// </summary>
/// <typeparam name="E">The type this token translates to:
/// <list type="bullet">
///   <item><description><c>CardEffect</c> - Top-level effect types (【永】, 【自】, 【起】, etc.)</description></item>
///   <item><description><c>List&lt;CardEffectAbility&gt;</c> - Individual ability clauses within effects</description></item>
///   <item><description><c>List&lt;CardEffectCondition&gt;</c> - Conditional clauses (when, if, during)</description></item>
///   <item><description><c>string</c> - Reminder text in parentheses</description></item>
/// </list>
/// </typeparam>
/// <remarks>
/// <para><b>Regex Conventions:</b></para>
/// <list type="number">
///   <item><description>All regex patterns MUST start with <c>^</c> (enforced by test)</description></item>
///   <item><description>Ability tokens MUST end with <c>(?:\.|,|、|。)?</c> to capture ending punctuation (enforced by test)</description></item>
///   <item><description>Condition tokens MUST be atomic - they should not capture conjunctions or multiple conditions (enforced by test)</description></item>
/// </list>
/// <para><b>When modifying regex patterns, expand the scope to handle more variations:</b></para>
/// <list type="bullet">
///   <item><description>Use <c>(?:pattern1|pattern2)</c> for alternative patterns</description></item>
///   <item><description>Use <c>\s*</c> for optional whitespace</description></item>
///   <item><description>Handle both full-width and half-width characters (e.g., <c>X</c> and <c>Ｘ</c>)</description></item>
///   <item><description>Add support for additional punctuation variants</description></item>
/// </list>
/// <para>See <c>README.md</c> in this directory for detailed documentation on expected clauses per token category.</para>
/// </remarks>
public abstract class CardTextToken<E>
{
    /// <summary>
    /// Regex pattern that matches the Japanese text clause this token recognizes.
    /// Must start with ^ anchor. Ability tokens must end with (?:\.|,|、|。)?
    /// </summary>
    public abstract Regex Matcher { get; }

    /// <summary>
    /// Translates the matched Japanese text into the structured representation.
    /// </summary>
    /// <param name="registry">The token registry for nested parsing of conditions/abilities</param>
    /// <param name="match">The matched text span from the regex</param>
    /// <returns>The translated representation of type E</returns>
    public abstract E Translate(ITokenRegistry registry, ReadOnlyMemory<char> match);
}

/// <summary>
/// Registry interface for token lookup and nested parsing during translation.
/// Provides access to the four token registries (effects, abilities, conditions, reminder text)
/// and label matching functionality.
/// </summary>
public interface ITokenRegistry
{
    /// <summary>Registry for ability tokens (List&lt;CardEffectAbility&gt;)</summary>
    IComponentRegistry<List<CardEffectAbility>> EffectListRegistry { get; }

    /// <summary>Registry for condition tokens (List&lt;CardEffectCondition&gt;)</summary>
    IComponentRegistry<List<CardEffectCondition>> ConditionListRegistry { get; }

    /// <summary>Registry for effect tokens (CardEffect)</summary>
    IComponentRegistry<CardEffect> EffectRegistry { get; }

    /// <summary>Registry for reminder text tokens (string)</summary>
    IComponentRegistry<string> ReminderTextRegistry { get; }

    /// <summary>
    /// Matches label patterns like 【R】, 【ターン1】, 【CXCOMBO】 and returns the label values.
    /// </summary>
    /// <param name="value">The label string to parse (e.g., "【R】【ターン1】")</param>
    /// <returns>Array of label values (e.g., ["R", "ターン1"])</returns>
    string[] MatchLabels(string value);
}

/// <summary>
/// Generic registry for managing and matching tokens of a specific type.
/// Tokens are matched in registration order, with the first match taking precedence.
/// </summary>
/// <typeparam name="E">The type this registry handles (CardEffect, List&lt;CardEffectAbility&gt;, etc.)</typeparam>
public interface IComponentRegistry<E>
{
    /// <summary>Returns all registered tokens in registration order</summary>
    IEnumerable<CardTextToken<E>> GetAllTokens();

    /// <summary>
    /// Matches input against all registered tokens. Only returns matches at index 0.
    /// </summary>
    /// <remarks>
    /// Strictness: Tokens matching at Index &gt; 0 are NOT returned. Instead, a warning
    /// is logged via Serilog.Log showing what text would be skipped and which tokens
    /// matched mid-string. This enforces that all parsing consumes from the start.
    /// Callers must handle null by either throwing or skipping known lead-in prefixes.
    /// </remarks>
    TokenMatchResult<E>? Match(ReadOnlyMemory<char> input);

    /// <summary>
    /// <para>Attempts to match a token at the start of the input string.</para>
    /// <para>Legacy — prefer <see cref="Match"/> for all new code.</para>
    /// </summary>
    [Obsolete("Use Match instead — only returns index-0 matches")]
    bool TryMatchAtStart(string input, out Func<ITokenRegistry, E>? result, out int consumedLength);

    /// <summary>
    /// <para>Finds the first token that matches anywhere in the input string.</para>
    /// <para>Legacy — prefer <see cref="Match"/> with prefix skipping.</para>
    /// </summary>
    [Obsolete("Use Match instead — only returns index-0 matches")]
    bool TryFindFirstMatch(string input, out Func<ITokenRegistry, E>? result, out int matchIndex, out int matchLength);

}
