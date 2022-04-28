using System;
using System.Runtime.Serialization;

namespace Montage.Card.API.Exceptions;

[Serializable]
public class DeckParsingException : Exception
{
    public DeckParsingException()
    {
    }

    public DeckParsingException(string message) : base(message)
    {
    }

    public DeckParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected DeckParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
