using BaroquenMelody.Library.Infrastructure.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Infrastructure.Extensions;

[TestFixture]
internal sealed class EnumExtensionsTests
{
    [Test]
    [TestCase(NoteName.A, "A")]
    [TestCase(NoteName.ASharp, "A Sharp")]
    [TestCase(NoteName.B, "B")]
    [TestCase(NoteName.C, "C")]
    [TestCase(NoteName.CSharp, "C Sharp")]
    [TestCase(NoteName.D, "D")]
    [TestCase(NoteName.DSharp, "D Sharp")]
    [TestCase(NoteName.E, "E")]
    [TestCase(NoteName.F, "F")]
    [TestCase(NoteName.FSharp, "F Sharp")]
    [TestCase(NoteName.G, "G")]
    [TestCase(NoteName.GSharp, "G Sharp")]
    public void ToSpaceSeparatedString_returns_expected_string(NoteName source, string expected)
    {
        // act
        var result = source.ToSpaceSeparatedString();

        // assert
        result.Should().Be(expected);
    }
}
