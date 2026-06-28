using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Scoring;

[TestFixture]
internal sealed class WeightedChordSelectorTests
{
    private const int Seed = 1234;

    private static readonly Note[] _distinctNotes =
    [
        Notes.C4, Notes.D4, Notes.E4, Notes.F4, Notes.G4, Notes.A4, Notes.B4, Notes.C5, Notes.D5, Notes.E5, Notes.F5, Notes.G5
    ];

    [Test]
    public void SelectNextChord_ReturnsNull_WhenThereAreNoCandidates()
    {
        // arrange
        var weightedChordSelector = new WeightedChordSelector(Substitute.For<IScoringRule>(), Substitute.For<IRandomProvider>());

        // act
        var selectedChord = weightedChordSelector.SelectNextChord([], []);

        // assert
        selectedChord.Should().BeNull();
    }

    [Test]
    public void SelectNextChord_ReturnsTheOnlyCandidate_WhenThereIsASingleCandidate()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };
        var candidate = BuildChord(Notes.D4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, candidate).Returns(3d);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, new SeededRandomProvider(Seed));

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidate]);

        // assert
        selectedChord.Should().BeSameAs(candidate);
    }

    [Test]
    public void SelectNextChord_StronglyFavorsTheLowestPenaltyCandidate()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var lowPenaltyChord = BuildChord(Notes.D4);
        var highPenaltyChord = BuildChord(Notes.E4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, lowPenaltyChord).Returns(0d);
        mockScoringRule.Score(precedingChords, highPenaltyChord).Returns(1000d);

        // even with the random target at the very top of its range, the negligible-weight candidate is not chosen.
        var mockRandomProvider = Substitute.For<IRandomProvider>();

        mockRandomProvider.Next().Returns(int.MaxValue - 1);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, mockRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [highPenaltyChord, lowPenaltyChord]);

        // assert
        selectedChord.Should().BeSameAs(lowPenaltyChord);
    }

    [Test]
    public void SelectNextChord_SometimesSelectsAHigherPenaltyCandidate_ButFavorsLowerPenaltyOnes()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var lowPenaltyChord = BuildChord(Notes.D4);
        var highPenaltyChord = BuildChord(Notes.E4);
        var candidates = new List<BaroquenChord> { lowPenaltyChord, highPenaltyChord };

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, lowPenaltyChord).Returns(0d);
        mockScoringRule.Score(precedingChords, highPenaltyChord).Returns(6d);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, new SeededRandomProvider(Seed));

        // act
        var counts = CountSelections(weightedChordSelector, precedingChords, candidates, iterations: 200);

        // assert: the softmax keeps the music moving by giving the rougher candidate a real chance, while still
        // preferring the smoother one (unlike a strict minimum-penalty pick, which would never choose the rougher one).
        counts[0].Should().BeGreaterThan(counts[1], "the lower-penalty candidate should be favored");
        counts[1].Should().BeGreaterThan(0, "the higher-penalty candidate should still be selected sometimes");
    }

    [Test]
    public void SelectNextChord_SelectsUniformlyAtRandom_WhenThereAreNoScoringRules()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };
        var candidates = _distinctNotes.Take(3).Select(BuildChord).ToList();

        var weightedChordSelector = new WeightedChordSelector(new AggregateScoringRule([]), new SeededRandomProvider(Seed));

        // act
        var counts = CountSelections(weightedChordSelector, precedingChords, candidates, iterations: 200);

        // assert: every candidate scores zero, so the distribution is uniform and every candidate is reachable.
        counts.Should().OnlyContain(count => count > 0, "with no scoring rules selection degrades to a uniform random pick");
    }

    [Test]
    public void SelectNextChord_IsDeterministicUnderASeed()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };
        var candidates = _distinctNotes.Take(5).Select(BuildChord).ToList();

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule
            .Score(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
            .Returns(callInfo => (double)IndexOfReference(candidates, callInfo.Arg<BaroquenChord>()));

        var firstSelector = new WeightedChordSelector(mockScoringRule, new SeededRandomProvider(Seed));
        var secondSelector = new WeightedChordSelector(mockScoringRule, new SeededRandomProvider(Seed));

        // act
        var firstSelections = Enumerable.Range(0, 50).Select(_ => firstSelector.SelectNextChord(precedingChords, candidates)).ToList();
        var secondSelections = Enumerable.Range(0, 50).Select(_ => secondSelector.SelectNextChord(precedingChords, candidates)).ToList();

        // assert
        firstSelections.Should().Equal(secondSelections);
    }

    private static int[] CountSelections(
        WeightedChordSelector weightedChordSelector,
        IReadOnlyList<BaroquenChord> precedingChords,
        List<BaroquenChord> candidates,
        int iterations)
    {
        var counts = new int[candidates.Count];

        for (var iteration = 0; iteration < iterations; ++iteration)
        {
            var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, candidates);

            counts[IndexOfReference(candidates, selectedChord!)]++;
        }

        return counts;
    }

    private static int IndexOfReference(List<BaroquenChord> chords, BaroquenChord chord)
    {
        for (var index = 0; index < chords.Count; ++index)
        {
            if (ReferenceEquals(chords[index], chord))
            {
                return index;
            }
        }

        return -1;
    }

    private static BaroquenChord BuildChord(Note note) => new([new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half)]);
}
