using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Contexts;

[TestFixture]
internal sealed class DuetChordContextRepositoryTests
{
    private INoteContextGenerator _mockNoteContextGenerator = null!;

    [SetUp]
    public void SetUp() => _mockNoteContextGenerator = Substitute.For<INoteContextGenerator>();

    [Test]
    public void WhenDuetChordContextRepositoryIsConstructed_ItGeneratesNoteContexts()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55.ToNote(), 90.ToNote()),
                new(Voice.Alto, 45.ToNote(), 80.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var noteContext1 = new NoteContext(Voice.Soprano, 60.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, 70.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Soprano, 65.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var noteContext4 = new NoteContext(Voice.Alto, 75.ToNote(), NoteMotion.Oblique, NoteSpan.None);

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { noteContext1, noteContext3 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { noteContext2, noteContext4 });

        var duetChordContextRepository = new DuetChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // act
        var chordContextId1 = duetChordContextRepository.GetChordContextId(
            new ChordContext(new[] { noteContext1, noteContext2 })
        );

        var chordContextId2 = duetChordContextRepository.GetChordContextId(
            new ChordContext(new[] { noteContext3, noteContext4 })
        );

        // assert
        chordContextId1.Should().Be(0);
        chordContextId2.Should().Be(3);

        Received.InOrder(() =>
            {
                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano),
                    Arg.Any<Scale>()
                );

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto),
                    Arg.Any<Scale>()
                );
            }
        );
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToDuetChordContextRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55.ToNote(), 90.ToNote()),
                new(Voice.Alto, 45.ToNote(), 80.ToNote()),
                new(Voice.Tenor, 35.ToNote(), 70.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var act = () => _ = new DuetChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }
}
