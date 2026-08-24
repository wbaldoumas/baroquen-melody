using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine;

[TestFixture]
internal sealed class OrnamentationEngineBuilderTests
{
    private OrnamentationEngineBuilder _builder = null!;

    [SetUp]
    public void SetUp()
    {
        var configuration = TestCompositionConfigurations.Get();

        _builder = new OrnamentationEngineBuilder(
            configuration,
            new MusicalTimeSpanCalculator(),
            new SeededRandomProvider(1),
            Substitute.For<ILogger>(),
            new VoiceRhythmLedger()
        );
    }

    [Test]
    public void BuildOrnamentationCleaner_CleansTheJustDecoratedNoteWhenItsPassingToneClashesOnTheStrongPulse()
    {
        // arrange - two passing tones sound together on the mid-beat pulse (a strong pulse in 4/4), a major second apart
        var cleaner = _builder.BuildOrnamentationCleaner();

        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter) }
        };

        var otherNote = new BaroquenNote(Instrument.Two, Notes.B2, MusicalTimeSpan.Quarter)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Quarter) }
        };

        // act
        cleaner.Process(new OrnamentationCleaningItem(note, otherNote));

        // assert
        note.OrnamentationType.Should().Be(OrnamentationType.None, "the just-decorated note loses a strong-pulse clash");
        note.Ornamentations.Should().BeEmpty();
        otherNote.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        otherNote.Ornamentations.Should().ContainSingle();
    }

    [Test]
    public void BuildOrnamentationCleaner_LeavesAnUncleanablePairAlone()
    {
        // arrange - an un-ornamented note has no cleaner registered against any partner
        var cleaner = _builder.BuildOrnamentationCleaner();

        var note = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);

        var otherNote = new BaroquenNote(Instrument.Two, Notes.B2, MusicalTimeSpan.Quarter)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Quarter) }
        };

        // act
        cleaner.Process(new OrnamentationCleaningItem(note, otherNote));

        // assert
        note.OrnamentationType.Should().Be(OrnamentationType.None);
        otherNote.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        otherNote.Ornamentations.Should().ContainSingle();
    }
}
