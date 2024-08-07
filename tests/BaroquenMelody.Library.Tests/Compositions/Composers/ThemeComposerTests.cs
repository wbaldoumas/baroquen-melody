﻿using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
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
internal sealed class ThemeComposerTests
{
    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordComposer _mockChordComposer = null!;

    private INoteTransposer _mockNoteTransposer = null!;

    private ILogger _mockLogger = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    private ThemeComposer _themeComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionStrategy = Substitute.For<ICompositionStrategy>();
        _mockCompositionDecorator = Substitute.For<ICompositionDecorator>();
        _mockChordComposer = Substitute.For<IChordComposer>();
        _mockNoteTransposer = Substitute.For<INoteTransposer>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = Configurations.GetCompositionConfiguration();

        _themeComposer = new ThemeComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordComposer, _mockNoteTransposer, _mockLogger, _compositionConfiguration);
    }

    [Test]
    public void WhenFugalThemeCannotBeComposed_FallbackBaroquenThemeIsReturned()
    {
        // arrange
        var mockBaroquenChord = new BaroquenChord([
            new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Voice.Bass, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(mockBaroquenChord);

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(new BaroquenChord(mockBaroquenChord));

        // act
        var result = _themeComposer.Compose();

        // assert
        result.Should().NotBeNull();
        result.Exposition.Should().NotBeEmpty();
        result.Recapitulation.Should().NotBeEmpty();
    }
}
