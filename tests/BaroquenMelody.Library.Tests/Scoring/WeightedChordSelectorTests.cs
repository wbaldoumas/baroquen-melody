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

        mockRandomProvider.Next().Returns(7, 3);

        var weightedChordSelector = new WeightedChordSelector(mockScoringRule, mockRandomProvider);

        // act
        var selectedChord = weightedChordSelector.SelectNextChord(precedingChords, [candidateA, candidateB, candidateC]);

        // assert: only the two tied candidates participate in the random tie-break, and the smaller key wins.
        selectedChord.Should().BeSameAs(candidateC);

        mockRandomProvider.Received(2).Next();
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
