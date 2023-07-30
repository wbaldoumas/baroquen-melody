using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Contexts;

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
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80)
            }
        );

        var noteContext1 = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var noteContext2 = new NoteContext(Voice.Alto, 70, NoteMotion.Oblique, NoteSpan.None);
        var noteContext3 = new NoteContext(Voice.Soprano, 65, NoteMotion.Oblique, NoteSpan.None);
        var noteContext4 = new NoteContext(Voice.Alto, 75, NoteMotion.Oblique, NoteSpan.None);

        _mockNoteContextGenerator
            .GenerateNoteContexts(
                Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano)
            )
            .Returns(new HashSet<NoteContext> { noteContext1, noteContext3 });

        _mockNoteContextGenerator
            .GenerateNoteContexts(Arg.Is<VoiceConfiguration>(vc => vc.Voice == Voice.Alto))
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
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Soprano)
                );

                _mockNoteContextGenerator.GenerateNoteContexts(
                    Arg.Is<VoiceConfiguration>(voiceConfiguration => voiceConfiguration.Voice == Voice.Alto)
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
                new(Voice.Soprano, 55, 90),
                new(Voice.Alto, 45, 80),
                new(Voice.Tenor, 35, 70)
            }
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
