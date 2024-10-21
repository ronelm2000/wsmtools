namespace Montage.Card.API.Utilities;
public static class MemoryExtensions
{
    public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(this T[]? array) => new ReadOnlyMemory<T>(array);
}
