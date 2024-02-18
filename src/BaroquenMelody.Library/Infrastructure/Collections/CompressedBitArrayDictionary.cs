using BaroquenMelody.Library.Infrastructure.Compression;
using System.Collections;
using System.IO.Compression;
using System.Numerics;

namespace BaroquenMelody.Library.Infrastructure.Collections;

internal sealed class CompressedBitArrayDictionary : IDictionary<BigInteger, BitArray>
{
    private readonly IBitArrayCompressor _compressor;

    private readonly IDictionary<BigInteger, byte[]> _innerDictionary;

    public CompressedBitArrayDictionary(IBitArrayCompressor compressor)
    {
        _compressor = compressor;
        _innerDictionary = new Dictionary<BigInteger, byte[]>();
    }

    public CompressedBitArrayDictionary()
    {
        _compressor = new BrotliBitArrayCompressor(CompressionLevel.Optimal);
        _innerDictionary = new Dictionary<BigInteger, byte[]>();
    }

    public ICollection<BigInteger> Keys => _innerDictionary.Keys;

    public ICollection<BitArray> Values => _innerDictionary.Values.Select(_compressor.Decompress).ToList();

    public int Count => _innerDictionary.Count;

    public bool IsReadOnly => false;

    public BitArray this[BigInteger key]
    {
        get => _compressor.Decompress(_innerDictionary[key]);
        set => _innerDictionary[key] = _compressor.Compress(value);
    }

    public void Add(BigInteger key, BitArray value)
    {
        if (_innerDictionary.ContainsKey(key))
        {
            throw new ArgumentException("An item with the same key has already been added.", nameof(key));
        }

        _innerDictionary.Add(key, _compressor.Compress(value));
    }

    public void Add(KeyValuePair<BigInteger, BitArray> item) => Add(item.Key, item.Value);

    public void Clear() => _innerDictionary.Clear();

    public bool Contains(KeyValuePair<BigInteger, BitArray> item) =>
        _innerDictionary.ContainsKey(item.Key) &&
        _innerDictionary[item.Key].SequenceEqual(_compressor.Compress(item.Value));

    public bool ContainsKey(BigInteger key) => _innerDictionary.ContainsKey(key);

    public void CopyTo(KeyValuePair<BigInteger, BitArray>[] items, int arrayIndex)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        if (arrayIndex < 0 || arrayIndex > items.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (items.Length - arrayIndex < Count)
        {
            throw new ArgumentException(
                "The number of elements in the source ICollection<T> is greater than the available space from arrayIndex to the end of the destination array.",
                nameof(arrayIndex)
            );
        }

        foreach (var item in this)
        {
            items[arrayIndex++] = item;
        }
    }

    public IEnumerator<KeyValuePair<BigInteger, BitArray>> GetEnumerator() => _innerDictionary.Select(item =>
        new KeyValuePair<BigInteger, BitArray>(item.Key, _compressor.Decompress(item.Value))
    ).GetEnumerator();

    public bool Remove(BigInteger key) => _innerDictionary.Remove(key);

    public bool Remove(KeyValuePair<BigInteger, BitArray> item) => _innerDictionary.Remove(item.Key);

    public bool TryGetValue(BigInteger key, out BitArray value)
    {
        if (_innerDictionary.TryGetValue(key, out var compressedValue))
        {
            value = _compressor.Decompress(compressedValue);
            return true;
        }

        value = default!;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
