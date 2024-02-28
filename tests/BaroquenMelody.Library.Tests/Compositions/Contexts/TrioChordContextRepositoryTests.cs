using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Contexts;

[TestFixture]
internal sealed class TrioChordContextRepositoryTests
{
    private INoteContextGenerator _mockNoteContextGenerator = null!;

    [SetUp]
    public void SetUp() => _mockNoteContextGenerator = Substitute.For<INoteContextGenerator>();

    [Test]
    public void WhenTrioChordContextRepositoryIsConstructed_ItGeneratesNoteContexts()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Note.Get(NoteName.A, 4), Note.Get(NoteName.A, 6)),
                new(Voice.Alto, Note.Get(NoteName.F, 3), Note.Get(NoteName.F, 5)),
                new(Voice.Tenor, Note.Get(NoteName.C, 3), Note.Get(NoteName.C, 5))
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var sopranoNoteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.A, 4), NoteMotion.Oblique, NoteSpan.None);
        var sopranoNoteContext2 = new NoteContext(Voice.Soprano, Note.Get(NoteName.A, 6), NoteMotion.Oblique, NoteSpan.None);

        var altoNoteContext1 = new NoteContext(Voice.Alto, Note.Get(NoteName.F, 3), NoteMotion.Oblique, NoteSpan.None);
        var altoNoteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.F, 5), NoteMotion.Oblique, NoteSpan.None);

        var tenorNoteContext1 = new NoteContext(Voice.Tenor, Note.Get(NoteName.C, 3), NoteMotion.Oblique, NoteSpan.None);
        var tenorNoteContext2 = new NoteContext(Voice.Tenor, Note.Get(NoteName.C, 5), NoteMotion.Oblique, NoteSpan.None);

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { sopranoNoteContext1, sopranoNoteContext2 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { altoNoteContext1, altoNoteContext2 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Tenor),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { tenorNoteContext1, tenorNoteContext2 });

        var trioChordContextRepository = new TrioChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // act
        var chordContextId1 = trioChordContextRepository.GetChordContextId(
            new ChordContext([sopranoNoteContext1, altoNoteContext1, tenorNoteContext1])
        );

        var chordContextId2 = trioChordContextRepository.GetChordContextId(
            new ChordContext([sopranoNoteContext2, altoNoteContext2, tenorNoteContext2])
        );

        // assert
        chordContextId1.Should().Be(0);

        // As the Cartesian product of three sets, the id for the second context in each set would be 7 ((2*2*2)-1).
        chordContextId2.Should().Be(7);

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

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Tenor),
                    Arg.Any<Scale>()
                );
            }
        );
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToTrioChordContextRepository_ItThrows()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Note.Get(NoteName.C, 4), Note.Get(NoteName.C, 6)),
                new(Voice.Alto, Note.Get(NoteName.C, 3), Note.Get(NoteName.C, 6))
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var act = () => _ = new TrioChordContextRepository(
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
        var sopranoVoiceConfiguration = new VoiceConfiguration(Voice.Soprano, Note.Get(NoteName.C, 4), Note.Get(NoteName.C, 6));
        var altoVoiceConfiguration = new VoiceConfiguration(Voice.Alto, Note.Get(NoteName.C, 3), Note.Get(NoteName.C, 5));
        var tenorVoiceConfiguration = new VoiceConfiguration(Voice.Tenor, Note.Get(NoteName.C, 2), Note.Get(NoteName.C, 4));

        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                sopranoVoiceConfiguration,
                altoVoiceConfiguration,
                tenorVoiceConfiguration
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var noteContext1 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, Note.Get(NoteName.C, 3), NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Tenor, Note.Get(NoteName.C, 2), NoteMotion.Oblique, NoteSpan.None);

        var noteContext4 = new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 1), NoteMotion.Oblique, NoteSpan.None);
        var noteContext5 = new NoteContext(Voice.Alto, Note.Get(NoteName.C, 0), NoteMotion.Oblique, NoteSpan.None);
        var noteContext6 = new NoteContext(Voice.Tenor, Note.Get(NoteName.C, 7), NoteMotion.Oblique, NoteSpan.None);

        _mockNoteContextGenerator
            .GenerateNoteContexts(sopranoVoiceConfiguration, Arg.Any<Scale>())
            .Returns(new HashSet<NoteContext> { noteContext1, noteContext4 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(altoVoiceConfiguration, Arg.Any<Scale>())
            .Returns(new HashSet<NoteContext> { noteContext2, noteContext5 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(tenorVoiceConfiguration, Arg.Any<Scale>())
            .Returns(new HashSet<NoteContext> { noteContext3, noteContext6 });

        var trioChordContextRepository = new TrioChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // act
        var chordContext1 = trioChordContextRepository.GetChordContext(0);
        var chordContext2 = trioChordContextRepository.GetChordContext(3);

        // assert
        chordContext1.NoteContexts.Should().BeEquivalentTo([noteContext1, noteContext2, noteContext3]);
        chordContext2.NoteContexts.Should().BeEquivalentTo([noteContext1, noteContext5, noteContext6]);
    }
}
