﻿using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class EndingComposerTests
{
    private EndingComposer _endingComposer = null!;

    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordNumberIdentifier _mockChordNumberIdentifier = null!;

    private ILogger _mockLogger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = Configurations.GetCompositionConfiguration();

        _mockChordNumberIdentifier = Substitute.For<IChordNumberIdentifier>();

        _endingComposer = new EndingComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordNumberIdentifier, _mockLogger, _compositionConfiguration);
    }

    [Test]
    public void Compose_GivenValidComposition_ReturnsComposedComposition()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 1)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V, ChordNumber.V, ChordNumber.I);

        // act
        var result = _endingComposer.Compose(composition, theme);

        // assert
        result.Should().NotBeNull();
        result.Measures.Should().NotBeEmpty();
        _mockCompositionDecorator.Received(2).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void Compose_WhenNoChordChoicesAvailable_UsesFallbackChordChoice()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();
        var bridgingChords = new List<BaroquenChord> { new([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)]) };

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([], bridgingChords);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.I);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V, ChordNumber.V, ChordNumber.I);

        var fallbackChordChoice = new ChordChoice(
        [
            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 1),
            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 1),
            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 1),
            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 1)
        ]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([fallbackChordChoice]);

        // act
        var result = _endingComposer.Compose(composition, theme);

        // assert
        result.Should().NotBeNull();
        result.Measures.Should().NotBeEmpty();
        _mockCompositionDecorator.Received(2).Decorate(Arg.Any<Composition>());
    }

    [Test]
    public void WhenMaxBridgingChordsAndMaxChordsToTonicAreReached_ThenCompositionIsStillReturned()
    {
        // arrange
        var composition = CreateTestComposition();
        var theme = CreateTestTheme();

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockCompositionStrategy.GetPossibleChordChoices(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns([new ChordChoice([new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)])]);

        _mockChordNumberIdentifier.IdentifyChordNumber(Arg.Any<BaroquenChord>())
            .Returns(ChordNumber.V, ChordNumber.V);

        // act
        var result = _endingComposer.Compose(composition, theme);

        // assert
        result.Should().NotBeNull();
    }

    private static Composition CreateTestComposition()
    {
        var measure = new Measure([new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)]))], Meter.FourFour);

        return new Composition([measure]);
    }

    private static BaroquenTheme CreateTestTheme()
    {
        var measure = new Measure([new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)]))], Meter.FourFour);

        return new BaroquenTheme([measure], [measure]);
    }
}
