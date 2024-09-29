using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;
using BaroquenMelody.Library.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Selection.Strategies;

[TestFixture]
internal sealed class CleanTargetOrnamentationTests
{
    private CleanTargetOrnamentation _cleanTargetOrnamentation = null!;

    [SetUp]
    public void SetUp() => _cleanTargetOrnamentation = new CleanTargetOrnamentation();

    [Test]
    public void CleanTargetOrnamentation_selects_correct_note_regardless_of_order()
    {
        // arrange
        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DecorateInterval
        };

        var otherNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DelayedPassingTone
        };

        // act
        var result = _cleanTargetOrnamentation.Select(note, otherNote);
        var otherResult = _cleanTargetOrnamentation.Select(otherNote, note);

        // assert
        result.Should().Be(otherNote);
        otherResult.Should().Be(otherNote);
    }

    [Test]
    public void CleanTargetOrnamentation_handles_all_ornamentation_combos()
    {
        var ornamentationTypes = EnumUtils<OrnamentationType>.AsEnumerable().ToList();

        foreach (var ornamentationType in ornamentationTypes)
        {
            foreach (var otherOrnamentationType in ornamentationTypes)
            {
                var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
                {
                    OrnamentationType = ornamentationType
                };

                var otherNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
                {
                    OrnamentationType = otherOrnamentationType
                };

                var act = () => _cleanTargetOrnamentation.Select(note, otherNote);

                act.Should().NotThrow();
            }
        }
    }
}
