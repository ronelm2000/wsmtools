using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.Card.API.Interfaces.Inputs;

/// <summary>
/// An an abstraction over the console, mostly used for testing and code coverage.
/// </summary>
public interface IConsole
{
    int WindowWidth { get; set; }
    bool IsOutputRedirected { get; }
    bool KeyAvailable { get; }

    event ConsoleCancelEventHandler? CancelKeyPress;

    ConsoleKeyInfo ReadKey(bool intercept);
    void Write(object? value);
    void WriteLine(string? message);
    Task<string> ReadToEndAsync(CancellationToken cancellationToken = default);
}
