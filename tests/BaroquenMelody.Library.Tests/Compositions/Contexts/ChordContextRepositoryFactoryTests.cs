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
internal sealed class ChordContextRepositoryFactoryTests
{
    private INoteContextGenerator _mockNoteContextGenerator = null!;
    private IChordContextRepositoryFactory _chordContextRepositoryFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNoteContextGenerator = Substitute.For<INoteContextGenerator>();
        _chordContextRepositoryFactory = new ChordContextRepositoryFactory(_mockNoteContextGenerator);
    }

    [Test]
    [TestCase(2, typeof(DuetChordContextRepository))]
    [TestCase(3, typeof(TrioChordContextRepository))]
    [TestCase(4, typeof(QuartetChordContextRepository))]
    public void WhenChordContextRepositoryFactoryCreatesChordContextRepository_ItReturnsExpectedType(
        int numberOfVoices,
        Type expectedType)
    {
        // arrange
        var compositionConfiguration = new CompositionConfiguration(
            Enumerable.Range(0, numberOfVoices)
                .Select(index => new VoiceConfiguration((Voice)index, index.ToNote(), (index + 1).ToNote()))
                .ToHashSet(),
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        // act
        var chordContextRepository = _chordContextRepositoryFactory.Create(compositionConfiguration);

        // assert
        chordContextRepository.Should().BeOfType(expectedType);
    }

    [Test]
    public void WhenChordContextRepositoryIsPassedInvalidConfiguration_ItThrows()
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
        var act = () => _chordContextRepositoryFactory.Create(compositionConfiguration);

        // assert
        act.Should().Throw<ArgumentException>();
    }
}
