namespace BaroquenMelody.Library.Infrastructure.Collections;

/// <summary>
///     Represents a list with a fixed size.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
internal interface IFixedSizeList<T> : IReadOnlyList<T>
{
    /// <summary>
    ///    Adds an item to the list. If the list is already at capacity, the first item is removed.
    /// </summary>
    /// <param name="item">The item to add to the list.</param>
    public void Add(T item);
}
