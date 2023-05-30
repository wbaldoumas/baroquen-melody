using BaroquenMelody.Library.Collections;
using BaroquenMelody.Library.Compression;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using System.Collections;

namespace BaroquenMelody.Library.Tests.Collections;

[TestFixture]
internal sealed class CompressedBitArrayDictionaryTests
{
    private IBitArrayCompressor _compressor = default!;
    private BitArray _bitArray = default!;
    private byte[] _compressedArray = default!;
    private CompressedBitArrayDictionary _dictionary = default!;

    [SetUp]
    public void Setup()
    {
        _compressor = Substitute.For<IBitArrayCompressor>();
        _bitArray = new BitArray(new[] { true, false, true, false, true });
        _compressedArray = new byte[] { 0b10101 };

        _compressor.Compress(_bitArray).Returns(_compressedArray);
        _compressor.Decompress(_compressedArray).Returns(_bitArray);

        _dictionary = new CompressedBitArrayDictionary(_compressor);
    }

    [Test]
    public void AddAndRetrieve_CompressedBitArrayDictionary()
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
    public void TryGetValue_KeyExists_ReturnsTrueAndCorrectValue()
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
    public void TryGetValue_KeyDoesNotExist_ReturnsFalseAndDefault()
    {
        // act
        var result = _dictionary.TryGetValue(2, out var value);

        // assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Test]
    public void Remove_KeyExists_ReturnsTrue()
    {
        // arrange
        _dictionary.Add(1, _bitArray);

        // act
        var result = _dictionary.Remove(1);

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void Remove_KeyDoesNotExist_ReturnsFalse()
    {
        // act
        var result = _dictionary.Remove(2);

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public void Clear_Dictionary_IsEmpty()
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
    public void Count_AfterAddingTwoItems_IsTwo()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Count.Should().Be(2);
    }

    [Test]
    public void Keys_AfterAddingTwoItems_ContainsBothKeys()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Keys.Should().Contain(new[] { 1, 2 });
    }

    [Test]
    public void Values_AfterAddingTwoItems_ContainsBothValues()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // assert
        _dictionary.Values.Should().HaveCount(2).And.OnlyContain(value => value.Equals(_bitArray));
    }

    [Test]
    public void IsReadOnly_ReturnsFalse()
    {
        // assert
        _dictionary.IsReadOnly.Should().BeFalse();
    }

    [Test]
    public void GetEnumerator_EnumeratesAllItems()
    {
        // arrange
        _dictionary.Add(1, _bitArray);
        _dictionary.Add(2, _bitArray);

        // act
        var enumeratedItems = _dictionary.ToList();

        // assert
        enumeratedItems.Should().Contain(
            new KeyValuePair<int, BitArray>(1, _bitArray),
            new KeyValuePair<int, BitArray>(2, _bitArray)
        );
    }
}
