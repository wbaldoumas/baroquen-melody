﻿using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class RepeatedNoteProcessorTests
{
    private IMusicalTimeSpanCalculator _mockMusicalTimeSpanCalculator;

    private RepeatedNoteProcessor _repeatedNoteProcessor;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        _mockMusicalTimeSpanCalculator = Substitute.For<IMusicalTimeSpanCalculator>();
        _repeatedNoteProcessor = new RepeatedNoteProcessor(_mockMusicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.RepeatedNote);
    }

    [Test]
    public void Process_applies_ornamentation_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half)]))
        );

        _mockMusicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth);
        _mockMusicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(Arg.Any<OrnamentationType>(), Arg.Any<Meter>()).Returns(MusicalTimeSpan.Eighth);

        // act
        _repeatedNoteProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.RepeatedNote);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }
}
