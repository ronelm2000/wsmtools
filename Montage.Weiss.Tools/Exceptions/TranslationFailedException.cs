namespace Montage.Weiss.Tools.Exceptions;

public class TranslationFailedException : Exception
{
    public TranslationFailedException(string message, AggregateException inner)
        : base(message, inner)
    {
    }
}
