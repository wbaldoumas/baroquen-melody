using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Choices;

[TestFixture]
internal sealed class ChordChoiceRepositoryFactoryTests
{
    private INoteChoiceGenerator _mockNoteChoiceGenerator = null!;

    private ChordChoiceRepositoryFactory _chordChoiceRepositoryFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteChoiceGenerator = Substitute.For<INoteChoiceGenerator>();
        _chordChoiceRepositoryFactory = new ChordChoiceRepositoryFactory(_mockNoteChoiceGenerator);
    }

    [Test]
    [TestCase(2, typeof(DuetChordChoiceRepository))]
    [TestCase(3, typeof(TrioChordChoiceRepository))]
    [TestCase(4, typeof(QuartetChordChoiceRepository))]
    public void WhenChordChoiceRepositoryFactoryCreatesChordChoiceRepository_ItReturnsExpectedType(
        int numberOfVoices,
        Type expectedType)
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            Enumerable.Range(0, numberOfVoices)
                .Select(index => new VoiceConfiguration(Voice.Soprano, index.ToNote(), (index + 1).ToNote()))
                .ToHashSet(),
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var chordChoiceRepository = _chordChoiceRepositoryFactory.Create(compositionConfiguration);

        // assert
        chordChoiceRepository.Should().BeOfType(expectedType);
    }

    [Test]
    public void WhenChordChoiceRepositoryIsPassedInvalidConfiguration_ItThrows()
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                // invalid configuration: only one voice
                new(Voice.Soprano, 55.ToNote(), 90.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var act = () => _chordChoiceRepositoryFactory.Create(compositionConfiguration);

        // assert
        act.Should().Throw<ArgumentException>();
    }
}
