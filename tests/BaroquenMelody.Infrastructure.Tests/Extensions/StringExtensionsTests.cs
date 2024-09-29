using BaroquenMelody.Infrastructure.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Extensions;

[TestFixture]
internal sealed class StringExtensionsTests
{
    [Test]
    [TestCase(nameof(NoteName.A), "A")]
    [TestCase(nameof(NoteName.ASharp), "A Sharp")]
    [TestCase(nameof(NoteName.B), "B")]
    [TestCase(nameof(NoteName.C), "C")]
    [TestCase(nameof(NoteName.CSharp), "C Sharp")]
    [TestCase(nameof(NoteName.D), "D")]
    [TestCase(nameof(NoteName.DSharp), "D Sharp")]
    [TestCase(nameof(NoteName.E), "E")]
    [TestCase(nameof(NoteName.F), "F")]
    [TestCase(nameof(NoteName.FSharp), "F Sharp")]
    [TestCase(nameof(NoteName.G), "G")]
    [TestCase(nameof(NoteName.GSharp), "G Sharp")]
    [TestCase(nameof(GeneralMidi2Program.AcousticGuitarNylon2), "Acoustic Guitar Nylon 2")]
    [TestCase(nameof(GeneralMidi2Program.SynthBass4Attack), "Synth Bass 4 Attack")]
    [TestCase(nameof(GeneralMidi2Program.Lead1ASquare2), "Lead 1 A Square 2")]
    public void ToSpaceSeparatedString_returns_expected_string(string source, string expected)
    {
        // act
        var result = source.ToSpaceSeparatedString();

        // assert
        result.Should().Be(expected);
    }
}
