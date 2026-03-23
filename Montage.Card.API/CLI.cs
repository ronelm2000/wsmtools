using Montage.Card.API.Interfaces.Inputs;

namespace Montage.Card.API;

public class CLI : IConsole
{
    public static IConsole Instance { get; } = new CLI();

    public int WindowWidth
    {
        get => Console.WindowWidth;
        set
        {
            if (OperatingSystem.IsWindows())
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
    public void Write(object? value) => Console.Write(value);
}



