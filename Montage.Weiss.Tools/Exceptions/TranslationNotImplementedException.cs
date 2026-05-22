using Montage.Weiss.Tools.Entities.Effect;

namespace Montage.Weiss.Tools.Exceptions;

/// <summary>
/// Thrown when effect text contains unrecognized conditions, abilities, or costs
/// that no registered token could parse. Carries the partially-built <see cref="CardEffect"/>
/// so callers can inspect what was successfully parsed before the failure.
/// </summary>
public class TranslationNotImplementedException : NotImplementedException
{
    public CardEffect Effect { get; }

    public TranslationNotImplementedException(string message, CardEffect effect)
        : base(message)
    {
        Effect = effect;
    }
}
