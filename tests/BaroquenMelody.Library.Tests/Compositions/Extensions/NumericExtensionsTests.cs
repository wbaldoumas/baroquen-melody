using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Extensions;

[TestFixture]
internal sealed class NumericExtensionsTests
{
    [Test]
    [TestCase(60, NoteName.C, 4)]
    [TestCase(61, NoteName.CSharp, 4)]
    [TestCase(62, NoteName.D, 4)]
    [TestCase(63, NoteName.DSharp, 4)]
    [TestCase(64, NoteName.E, 4)]
    [TestCase(65, NoteName.F, 4)]
    [TestCase(66, NoteName.FSharp, 4)]
    [TestCase(67, NoteName.G, 4)]
    [TestCase(68, NoteName.GSharp, 4)]
    [TestCase(69, NoteName.A, 4)]
    [TestCase(70, NoteName.ASharp, 4)]
    [TestCase(71, NoteName.B, 4)]
    [TestCase(72, NoteName.C, 5)]
    public void ToNoteWithInteger_GeneratesExpectedNote(int pitch, NoteName expectedNoteName, int expectedOctave)
    {
        // act
        var note = pitch.ToNote();

        // assert
        note.NoteName.Should().Be(expectedNoteName);
        note.Octave.Should().Be(expectedOctave);
    }

    [Test]
    [TestCase((byte)60, NoteName.C, 4)]
    [TestCase((byte)61, NoteName.CSharp, 4)]
    [TestCase((byte)62, NoteName.D, 4)]
    [TestCase((byte)63, NoteName.DSharp, 4)]
    [TestCase((byte)64, NoteName.E, 4)]
    [TestCase((byte)65, NoteName.F, 4)]
    [TestCase((byte)66, NoteName.FSharp, 4)]
    [TestCase((byte)67, NoteName.G, 4)]
    [TestCase((byte)68, NoteName.GSharp, 4)]
    [TestCase((byte)69, NoteName.A, 4)]
    [TestCase((byte)70, NoteName.ASharp, 4)]
    [TestCase((byte)71, NoteName.B, 4)]
    [TestCase((byte)72, NoteName.C, 5)]
    public void ToNoteWithByte_GeneratesExpectedNote(byte pitch, NoteName expectedNoteName, int expectedOctave)
    {
        // act
        var note = pitch.ToNote();

        // assert
        note.NoteName.Should().Be(expectedNoteName);
        note.Octave.Should().Be(expectedOctave);
    }
}
