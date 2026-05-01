using Montage.Card.API.Interfaces.Inputs;

namespace Montage.Card.API;

public class CLI : IConsole
{
    public static IConsole Instance { get; } = new CLI();

    public int WindowWidth
    {
        get => !IsOutputRedirected ? Console.WindowWidth : 0;
        set
        {
            if (OperatingSystem.IsWindows() && !IsOutputRedirected)
                Console.WindowWidth = value;
        }
    }

    private CLI()
    {
        Console.CancelKeyPress += (s, e) => CancelKeyPress?.Invoke(s, e);
    }

    public event ConsoleCancelEventHandler? CancelKeyPress;
    public void WriteLine(string? message) => Console.WriteLine(message);
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);
    public bool IsOutputRedirected => Console.IsOutputRedirected;
    public bool KeyAvailable => Console.KeyAvailable;
    public void Write(object? value) => Console.Write(value);

    public async Task<string> ReadToEndAsync(CancellationToken cancellationToken = default)
    {
        return await Console.In.ReadToEndAsync(cancellationToken);
    }
}



