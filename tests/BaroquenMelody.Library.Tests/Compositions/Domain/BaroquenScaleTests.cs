using BaroquenMelody.Library.Compositions.Domain;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Domain;

[TestFixture]
internal sealed class BaroquenScaleTests
{
    [Test]
    public void ScaleDegrees_ReturnExpectedNoteNames()
    {
        // arrange
        var scale = BaroquenScale.Parse("C Major");

        // act + assert
        scale.Tonic.Should().Be(NoteName.C);
        scale.Supertonic.Should().Be(NoteName.D);
        scale.Mediant.Should().Be(NoteName.E);
        scale.Subdominant.Should().Be(NoteName.F);
        scale.Dominant.Should().Be(NoteName.G);
        scale.Submediant.Should().Be(NoteName.A);
        scale.LeadingTone.Should().Be(NoteName.B);
    }
}
