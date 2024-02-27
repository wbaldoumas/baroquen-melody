using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Infrastructure.Compression;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Infrastructure.Collections;

[TestFixture]
internal sealed class CompressedBitArrayDictionaryTests
{
    private static readonly bool[] AlternatingBitArrayValues = [true, false, true, false, true];

    private IBitArrayCompressor _compressor = default!;

    private BitArray _bitArray = default!;

    private byte[] _compressedArray = default!;

    private CompressedBitArrayDictionary _dictionary = default!;

    [SetUp]
    public void Setup()
    {
        _compressor = Substitute.For<IBitArrayCompressor>();
        _bitArray = new BitArray(AlternatingBitArrayValues);
        _compressedArray = [0b10101];

        _compressor.Compress(_bitArray).Returns(_compressedArray);
        _compressor.Decompress(_compressedArray).Returns(_bitArray);

        _dictionary = new CompressedBitArrayDictionary(_compressor);
    }

    [Test]
    public void AddAndRetrieve_results_in_identical_BitArray()
    {
        // act
        _dictionary.Add(1, _bitArray);
        var retrievedBitArray = _dictionary[1];

        // assert
        retrievedBitArray.Should().BeEquivalentTo(_bitArray);

        _compressor.Received(1).Compress(_bitArray);
        _compressor.Received(1).Decompress(_compressedArray);
    }

    [Test]
    public void AddAndRetrieve_with_KeyValuePair_results_in_identical_BitArray()
    {
        // act
        _dictionary.Add(new KeyValuePair<BigInteger, BitArray>(1, _bitArray));
        var retrievedBitArray = _dictionary[1];

        // assert
        retrievedBitArray.Should().BeEquivalentTo(_bitArray);

        _compressor.Received(1).Compress(_bitArray);
        _compressor.Received(1).Decompress(_compressedArray);
    }

    [Test]
    public void AddAndRetrieve_with_indexer_results_in_identical_BitArray()
    {
        // act
        _dictionary[1] = _bitArray;
        var retrievedBitArray = _dictionary[1];

        // assert
        retrievedBitArray.Should().BeEquivalentTo(_bitArray);

        _compressor.Received(1).Compress(_bitArray);
        _compressor.Received(1).Decompress(_compressedArray);
    }

    [Test]
    public void Add_throws_exception_when_key_already_exists()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var act = () => _dictionary.Add(1, _bitArray);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CopyTo_copies_items_to_array()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var array = new KeyValuePair<BigInteger, BitArray>[1];
        _dictionary.CopyTo(array, 0);

        // assert
        array[0].Key.Should().Be(1);
        array[0].Value.Should().BeEquivalentTo(_bitArray);
    }

    [Test]
    public void CopyTo_throws_ArgumentNullException_when_items_is_null()
    {
        // act
        var act = () => _dictionary.CopyTo(null!, 0);

        // assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void CopyTo_throws_ArgumentOutOfRangeException_when_index_is_negative()
    {
        // act
        var act = () => _dictionary.CopyTo(new KeyValuePair<BigInteger, BitArray>[1], -1);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CopyTo_throws_ArgumentOutOfRangeException_when_index_is_greater_than_array_length()
    {
        // act
        var act = () => _dictionary.CopyTo(new KeyValuePair<BigInteger, BitArray>[1], 100);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void CopyTo_throws_ArgumentException_when_number_of_elements_in_dictionary_is_greater_than_array_length()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // act
        var act = () => _dictionary.CopyTo(new KeyValuePair<BigInteger, BitArray>[1], 0);

        // assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Contains_returns_true_when_key_exists()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var result = _dictionary.Contains(new KeyValuePair<BigInteger, BitArray>(1, _bitArray));

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void Contains_returns_false_when_key_does_not_exist()
    {
        // act
        var result = _dictionary.Contains(new KeyValuePair<BigInteger, BitArray>(1, _bitArray));

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public void ContainsKey_returns_true_when_key_exists()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var result = _dictionary.ContainsKey(1);

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void ContainsKey_returns_false_when_key_does_not_exist()
    {
        // act
        var result = _dictionary.ContainsKey(1);

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public void Remove_removes_item()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        _dictionary.Remove(1);

        // assert
        _dictionary.Should().BeEmpty();
    }

    [Test]
    public void Remove_with_KeyValuePair_removes_item()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        _dictionary.Remove(new KeyValuePair<BigInteger, BitArray>(1, _bitArray));

        // assert
        _dictionary.Should().BeEmpty();
    }

    [Test]
    public void GetEnumerator_returns_enumerator()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        IEnumerator enumerator = _dictionary.GetEnumerator();

        // assert
        enumerator.Should().NotBeNull();

        (enumerator as IDisposable)?.Dispose();
    }

    [Test]
    public void IEnumerable_GetEnumerator_returns_enumerator()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var enumerator = ((IEnumerable)_dictionary).GetEnumerator();

        // assert
        enumerator.Should().NotBeNull();

        (enumerator as IDisposable)?.Dispose();
    }

    [Test]
    public void TryGetValue_returns_true_and_correct_value()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var result = _dictionary.TryGetValue(1, out var value);

        // assert
        result.Should().BeTrue();
        value.Should().BeEquivalentTo(_bitArray);
    }

    [Test]
    public void TryGetValue_returns_false_and_null_value()
    {
        // act
        var result = _dictionary.TryGetValue(2, out var value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Test]
    public void Remove_returns_true_when_key_exists()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var result = _dictionary.Remove(1);

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void Remove_returns_false_when_key_does_not_exist()
    {
        // act
        var result = _dictionary.Remove(2);

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public void Clear_clears_dictionary()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // act
        _dictionary.Clear();

        // assert
        _dictionary.Should().BeEmpty();
    }

    [Test]
    public void Count_returns_expected_value()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Count.Should().Be(2);
    }

    [Test]
    public void Keys_returns_correct_keys()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Keys.Should().Contain(new BigInteger[] { 1, 2 });
    }

    [Test]
    public void Values_returns_correct_values()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Values.Should().HaveCount(2).And.OnlyContain(value => value.Equals(_bitArray));
    }

    [Test]
    public void IsReadOnly_returns_false()
    {
        // assert
        _dictionary.IsReadOnly.Should().BeFalse();
    }

    [Test]
    public void GetEnumerator_enumerates_all_items()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // act
        var enumeratedItems = _dictionary.ToList();

        // assert
        enumeratedItems.Should().Contain(
            new KeyValuePair<BigInteger, BitArray>(1, _bitArray),
            new KeyValuePair<BigInteger, BitArray>(2, _bitArray)
        );
    }

    [Test]
    public void DefaultConstructor_generates_usable_instance()
    {
        // arrange + act
        var act = () => new CompressedBitArrayDictionary();

        // assert
        act.Should().NotThrow();
    }
}
