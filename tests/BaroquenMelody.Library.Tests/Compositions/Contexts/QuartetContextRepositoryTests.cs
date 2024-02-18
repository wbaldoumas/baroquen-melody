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
internal sealed class QuartetChordContextRepositoryTests
{
    private INoteContextGenerator _mockNoteContextGenerator = null!;

    [SetUp]
    public void SetUp() => _mockNoteContextGenerator = Substitute.For<INoteContextGenerator>();

    [Test]
    public void WhenQuartetChordContextRepositoryIsConstructed_ItGeneratesNoteContexts()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, 55.ToNote(), 90.ToNote()),
                new(Voice.Alto, 45.ToNote(), 80.ToNote()),
                new(Voice.Tenor, 35.ToNote(), 70.ToNote()),
                new(Voice.Bass, 25.ToNote(), 60.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var sopranoNoteContext1 = new NoteContext(Voice.Soprano, 60.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var sopranoNoteContext2 = new NoteContext(Voice.Soprano, 65.ToNote(), NoteMotion.Oblique, NoteSpan.None);

        var altoNoteContext1 = new NoteContext(Voice.Alto, 70.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var altoNoteContext2 = new NoteContext(Voice.Alto, 75.ToNote(), NoteMotion.Oblique, NoteSpan.None);

        var tenorNoteContext1 = new NoteContext(Voice.Tenor, 40.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var tenorNoteContext2 = new NoteContext(Voice.Tenor, 45.ToNote(), NoteMotion.Oblique, NoteSpan.None);

        var bassNoteContext1 = new NoteContext(Voice.Bass, 30.ToNote(), NoteMotion.Oblique, NoteSpan.None);
        var bassNoteContext2 = new NoteContext(Voice.Bass, 35.ToNote(), NoteMotion.Oblique, NoteSpan.None);

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

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Bass),
                Arg.Any<Scale>()
            )
            .Returns(new HashSet<NoteContext> { bassNoteContext1, bassNoteContext2 });

        var quartetChordContextRepository = new QuartetChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // act
        var chordContextId1 = quartetChordContextRepository.GetChordContextIndex(
            new ChordContext(new[] { sopranoNoteContext1, altoNoteContext1, tenorNoteContext1, bassNoteContext1 })
        );

        var chordContextId2 = quartetChordContextRepository.GetChordContextIndex(
            new ChordContext(new[] { sopranoNoteContext2, altoNoteContext2, tenorNoteContext2, bassNoteContext2 })
        );

        // assert
        chordContextId1.Should().Be(0);

        // As the Cartesian product of four sets, the index for the second context in each set would be 15 ((2*2*2*2)-1).
        chordContextId2.Should().Be(15);

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

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Bass),
                    Arg.Any<Scale>()
                );
            }
        );
    }

    [Test]
    public void WhenInvalidCompositionConfigurationIsPassedToQuartetChordContextRepository_ItThrows()
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
        var act = () => _ = new QuartetChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // assert
        act.Should().Throw<ArgumentException>();
    }
}
