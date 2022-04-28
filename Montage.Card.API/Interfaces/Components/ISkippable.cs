namespace Montage.Card.API.Interfaces.Components;

/// <summary>
/// This interface signifies that a post-processor can be skipped when executed with T information. T will be related to its CLI counterpart.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ISkippable<T>
{
    Task<bool> IsIncluded(T info);
}
