using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Contexts;

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
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80),
                new(Voice.Tenor, 35, 70),
                new(Voice.Bass, 25, 60)
            }
        );

        var sopranoNoteContext1 = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var sopranoNoteContext2 = new NoteContext(Voice.Soprano, 65, NoteMotion.Oblique, NoteSpan.None);

        var altoNoteContext1 = new NoteContext(Voice.Alto, 70, NoteMotion.Oblique, NoteSpan.None);
        var altoNoteContext2 = new NoteContext(Voice.Alto, 75, NoteMotion.Oblique, NoteSpan.None);

        var tenorNoteContext1 = new NoteContext(Voice.Tenor, 40, NoteMotion.Oblique, NoteSpan.None);
        var tenorNoteContext2 = new NoteContext(Voice.Tenor, 45, NoteMotion.Oblique, NoteSpan.None);

        var bassNoteContext1 = new NoteContext(Voice.Bass, 30, NoteMotion.Oblique, NoteSpan.None);
        var bassNoteContext2 = new NoteContext(Voice.Bass, 35, NoteMotion.Oblique, NoteSpan.None);

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano)
            )
            .Returns(new HashSet<NoteContext> { sopranoNoteContext1, sopranoNoteContext2 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto)
            )
            .Returns(new HashSet<NoteContext> { altoNoteContext1, altoNoteContext2 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Tenor)
            )
            .Returns(new HashSet<NoteContext> { tenorNoteContext1, tenorNoteContext2 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Bass)
            )
            .Returns(new HashSet<NoteContext> { bassNoteContext1, bassNoteContext2 });

        var quartetChordContextRepository = new QuartetChordContextRepository(
            compositionConfiguration,
            _mockNoteContextGenerator
        );

        // act
        var chordContextId1 = quartetChordContextRepository.GetChordContextId(
            new ChordContext(new[] { sopranoNoteContext1, altoNoteContext1, tenorNoteContext1, bassNoteContext1 })
        );

        var chordContextId2 = quartetChordContextRepository.GetChordContextId(
            new ChordContext(new[] { sopranoNoteContext2, altoNoteContext2, tenorNoteContext2, bassNoteContext2 })
        );

        // assert
        chordContextId1.Should().Be(0);

        // As the Cartesian product of four sets, the index for the second context in each set would be 15 ((2*2*2*2)-1).
        chordContextId2.Should().Be(15);

        Received.InOrder(() =>
            {
                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano)
                );

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto)
                );

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Tenor)
                );

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Bass)
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
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80),
                new(Voice.Tenor, 35, 70)
            }
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
