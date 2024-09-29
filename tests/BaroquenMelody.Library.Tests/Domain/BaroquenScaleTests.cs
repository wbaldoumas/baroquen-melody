using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Domain;

[TestFixture]
internal sealed class BaroquenScaleTests
{
    [Test]
    public void ScaleDegrees_ReturnExpectedNoteNames()
    {
        // arrange
        var scale = new BaroquenScale(NoteName.C, Mode.Ionian);

        // act + assert
        scale.Tonic.Should().Be(NoteName.C);
        scale.Supertonic.Should().Be(NoteName.D);
        scale.Mediant.Should().Be(NoteName.E);
        scale.Subdominant.Should().Be(NoteName.F);
        scale.Dominant.Should().Be(NoteName.G);
        scale.Submediant.Should().Be(NoteName.A);
        scale.LeadingTone.Should().Be(NoteName.B);
    }

    [Test]
    public void Construction_via_note_name_and_mode_creates_expected_scale()
    {
        // arrange
        const NoteName tonic = NoteName.C;
        const Mode mode = Mode.Ionian;

        // act
        var scale = new BaroquenScale(tonic, mode);

        // assert
        scale.Tonic.Should().Be(NoteName.C);
        scale.Supertonic.Should().Be(NoteName.D);
        scale.Mediant.Should().Be(NoteName.E);
        scale.Subdominant.Should().Be(NoteName.F);
        scale.Dominant.Should().Be(NoteName.G);
        scale.Submediant.Should().Be(NoteName.A);
        scale.LeadingTone.Should().Be(NoteName.B);
    }
}
