using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring;
using CsCheck;
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
    public void SelectNextChord_ReturnsTheMinimumPenaltyCandidate()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var candidateA = BuildChord(Notes.D4);
        var candidateB = BuildChord(Notes.E4);
        var candidateC = BuildChord(Notes.F4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, candidateA).Returns(2d);
        mockScoringRule.Score(precedingChords, candidateB).Returns(0d);
        mockScoringRule.Score(precedingChords, candidateC).Returns(1d);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, Substitute.For<IRandomProvider>());

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidateA, candidateB, candidateC]);

        // assert
        selectedChord.Should().BeSameAs(candidateB);
    }

    [Test]
    public void SelectNextChord_BreaksTiesWithTheRandomProvider()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var candidateA = BuildChord(Notes.D4);
        var candidateB = BuildChord(Notes.E4);
        var candidateC = BuildChord(Notes.F4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, candidateA).Returns(0d);
        mockScoringRule.Score(precedingChords, candidateB).Returns(5d);
        mockScoringRule.Score(precedingChords, candidateC).Returns(0d);

        var mockRandomProvider = Substitute.For<IRandomProvider>();

        mockRandomProvider.Next().Returns(7, 9, 3);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, mockRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidateA, candidateB, candidateC]);

        // assert: every candidate draws a tie-break key, and among the tied minimum-penalty candidates the smaller key wins.
        selectedChord.Should().BeSameAs(candidateC);

        mockRandomProvider.Received(3).Next();
    }

    [Test]
    public void SelectNextChord_BreaksTiesAtNonZeroPenalties()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var candidateA = BuildChord(Notes.D4);
        var candidateB = BuildChord(Notes.E4);
        var candidateC = BuildChord(Notes.F4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, candidateA).Returns(4d);
        mockScoringRule.Score(precedingChords, candidateB).Returns(4d);
        mockScoringRule.Score(precedingChords, candidateC).Returns(7d);

        var mockRandomProvider = Substitute.For<IRandomProvider>();

        mockRandomProvider.Next().Returns(8, 2, 5);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, mockRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidateA, candidateB, candidateC]);

        // assert: the tie at the minimum (non-zero) penalty is broken by the smaller key; the worse candidate never wins.
        selectedChord.Should().BeSameAs(candidateB);
    }

    [Test]
    public void SelectNextChord_WithoutScoringRules_MatchesTheLegacyRandomPickDrawForDraw()
    {
        // arrange: production candidate streams are lazy and may themselves consume random draws while being
        // enumerated (rule-bypass strictness draws from the same provider), so the selector must interleave its
        // tie-break draws with enumeration exactly like the legacy MinByRandom pick did.
        const int seed = 1234;

        var candidates = _distinctNotes.Take(6).Select(BuildChord).ToList();

        static IEnumerable<BaroquenChord> DrawConsumingCandidates(List<BaroquenChord> source, IRandomProvider randomProvider)
        {
            foreach (var chord in source)
            {
                _ = randomProvider.Next();

                yield return chord;
            }
        }

        var legacyRandomProvider = new SeededRandomProvider(seed);
        var legacyPick = DrawConsumingCandidates(candidates, legacyRandomProvider).MinByRandom(legacyRandomProvider);

        var selectorRandomProvider = new SeededRandomProvider(seed);
        var weightedChordSelector = new WeightedChordSelector(new AggregateScoringRule([]), selectorRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord([], DrawConsumingCandidates(candidates, selectorRandomProvider));

        // assert
        selectedChord.Should().BeSameAs(legacyPick);
    }

    [Test]
    public void SelectNextChord_FallsBackToUniformRandomSelection_WhenThereAreNoScoringRules()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };

        var candidateA = BuildChord(Notes.D4);
        var candidateB = BuildChord(Notes.E4);
        var candidateC = BuildChord(Notes.F4);

        var mockRandomProvider = Substitute.For<IRandomProvider>();

        mockRandomProvider.Next().Returns(5, 1, 9);

        var weightedChordSelector = new WeightedChordSelector(new AggregateScoringRule([]), mockRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidateA, candidateB, candidateC]);

        // assert: every candidate ties at zero, so selection degrades to the legacy uniform random pick.
        selectedChord.Should().BeSameAs(candidateB);

        mockRandomProvider.Received(3).Next();
    }

    [Test]
    public void SelectNextChord_AlwaysSelectsACandidateWithTheMinimumPenalty()
    {
        Gen.Select(Gen.Int[0, 5].List[1, 12], Gen.Int, static (penalties, seed) => (Penalties: penalties, Seed: seed)).Sample(
            input =>
            {
                // arrange
                var candidates = input.Penalties.Select((_, index) => BuildChord(_distinctNotes[index])).ToList();

                var mockScoringRule = Substitute.For<IScoringRule>();

                mockScoringRule
                    .Score(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>())
                    .Returns(callInfo => input.Penalties[IndexOfReference(candidates, callInfo.Arg<BaroquenChord>())]);

                var weightedChordSelector = new WeightedChordSelector(mockScoringRule, new SeededRandomProvider(input.Seed));

                // act
                var selectedChord = weightedChordSelector.SelectNextChord([], candidates);

                // assert
                selectedChord.Should().NotBeNull();

                input.Penalties[IndexOfReference(candidates, selectedChord)].Should().Be(input.Penalties.Min());
            },
            iter: 25
        );
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
