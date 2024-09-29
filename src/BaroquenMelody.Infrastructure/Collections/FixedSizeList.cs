using JetBrains.Annotations;
using System.Collections;

namespace BaroquenMelody.Infrastructure.Collections;

/// <inheritdoc cref="IFixedSizeList{T}"/>
public sealed class FixedSizeList<T> : IFixedSizeList<T>
{
    private readonly int _size;

    private readonly IList<T> _items;

    public FixedSizeList(int size)
    {
        _size = size;
        _items = new List<T>(size);
    }

    public FixedSizeList(int size, IEnumerable<T> items)
    {
        _size = size;
        _items = new List<T>(size);

        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Add(T item)
    {
        if (_items.Count == _size)
        {
            _items.RemoveAt(0);
        }

        _items.Add(item);
    }

    [MustDisposeResource]
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _items.Count;

    public T this[int index] => _items[index];
}
