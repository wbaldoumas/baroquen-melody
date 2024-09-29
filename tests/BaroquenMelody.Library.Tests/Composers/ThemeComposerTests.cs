using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

[TestFixture]
internal sealed class ThemeComposerTests
{
    private ICompositionStrategy _mockCompositionStrategy = null!;

    private ICompositionDecorator _mockCompositionDecorator = null!;

    private IChordComposer _mockChordComposer = null!;

    private INoteTransposer _mockNoteTransposer = null!;

    private IDispatcher _mockDispatcher = null!;

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
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration();

        _themeComposer = new ThemeComposer(_mockCompositionStrategy, _mockCompositionDecorator, _mockChordComposer, _mockNoteTransposer, _mockDispatcher, _mockLogger, _compositionConfiguration);
    }

    [Test]
    public void WhenFugalThemeCannotBeComposed_FallbackBaroquenThemeIsReturned()
    {
        // arrange
        var mockBaroquenChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.C1, MusicalTimeSpan.Half)
        ]);

        _mockCompositionStrategy.GenerateInitialChord().Returns(mockBaroquenChord);

        _mockCompositionStrategy.GetPossibleChordsForPartiallyVoicedChords(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns([]);

        _mockChordComposer.Compose(Arg.Any<IReadOnlyList<BaroquenChord>>())
            .Returns(new BaroquenChord(mockBaroquenChord));

        // act
        var result = _themeComposer.Compose(CancellationToken.None);

        // assert
        result.Should().NotBeNull();
        result.Exposition.Should().NotBeEmpty();
        result.Recapitulation.Should().NotBeEmpty();
    }
}
