using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Card.API.Exceptions;
public class SetParsingException : Exception
{
    public ParserCode? SourceCode { get; }

    public SetParsingException()
    {
    }
    public SetParsingException(ParserCode code) : base (code.Translate())
    {
        this.SourceCode = code;
    }

    public SetParsingException(string? message) : base(message)
    {
    }

    public SetParsingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SetParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

public abstract record ParserCode
{
    public virtual int Code { get; init; }
    public string[] Arguments { get; init; } = Array.Empty<string>();

    public abstract string Translate();
}

public record CannotBeParsedCode : ParserCode
{
    public override int Code { get; init; } = 1;
    public override string Translate() => $"{Arguments[0]} cannot be parsed.";

    public CannotBeParsedCode(params string[] arguments)
    {
        Arguments = arguments;
    }
}