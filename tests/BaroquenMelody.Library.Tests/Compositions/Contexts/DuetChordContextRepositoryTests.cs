using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
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
                new(Voice.Soprano, Note.Get(NoteName.A, 4), Note.Get(NoteName.A, 6)),
                new(Voice.Alto, Note.Get(NoteName.F, 3), Note.Get(NoteName.F, 5))
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.A, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.F, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Soprano, Note.Get(NoteName.A, 5), NoteMotion.Oblique, NoteSpan.None);
        var noteContext4 = new NoteContext(Voice.Alto, Note.Get(NoteName.F, 4), NoteMotion.Oblique, NoteSpan.None);

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
            new ChordContext([noteContext1, noteContext2])
        );

        var chordContextId2 = duetChordContextRepository.GetChordContextId(
            new ChordContext([noteContext3, noteContext4])
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
                new(Voice.Soprano, Note.Get(NoteName.C, 4), Note.Get(NoteName.C, 6)),
                new(Voice.Alto, Note.Get(NoteName.C, 3), Note.Get(NoteName.C, 6)),
                new(Voice.Tenor, Note.Get(NoteName.C, 2), Note.Get(NoteName.C, 3))
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

    [Test]
    public void WhenGetChordContext_ThenChordContextIsReturned()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Note.Get(NoteName.C, 4), Note.Get(NoteName.C, 6)),
                new(Voice.Alto, Note.Get(NoteName.C, 3), Note.Get(NoteName.C, 5))
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.C, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 2), NoteMotion.Oblique, NoteSpan.None);
        var noteContext4 = new NoteContext(Voice.Alto, Note.Get(NoteName.C, 1), NoteMotion.Oblique, NoteSpan.None);

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
        var chordContext1 = duetChordContextRepository.GetChordContext(0);
        var chordContext2 = duetChordContextRepository.GetChordContext(3);

        // assert
        chordContext1.NoteContexts.Should().BeEquivalentTo(new[] { noteContext1, noteContext2 });
        chordContext2.NoteContexts.Should().BeEquivalentTo(new[] { noteContext3, noteContext4 });
    }
}
