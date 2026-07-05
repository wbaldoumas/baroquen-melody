using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Numerics;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Choices;

[TestFixture]
internal sealed class ForwardCheckingChordChoiceEnumeratorTests
{
    private static readonly Note[] _midRangeAnchors = [Notes.C5, Notes.G3, Notes.F2, Notes.F1];

    private static readonly Note[] _maxRangeAnchors = [Notes.C6, Notes.G4, Notes.C3, Notes.C2];

    private static readonly Note[] _minRangeAnchors = [Notes.C4, Notes.G2, Notes.C2, Notes.C1];

    [Test]
    public void EnumerateCandidates_FromMidRangeAnchors_MatchesTheRangeFilteredRepositorySequence([Range(1, 4)] int numberOfInstruments)
    {
        AssertParityWithTheRepositoryOracle(numberOfInstruments, _midRangeAnchors);
    }

    [Test]
    public void EnumerateCandidates_FromTopOfRangeAnchors_MatchesTheRangeFilteredRepositorySequence([Range(1, 4)] int numberOfInstruments)
    {
        AssertParityWithTheRepositoryOracle(numberOfInstruments, _maxRangeAnchors);
    }

    [Test]
    public void EnumerateCandidates_FromBottomOfRangeAnchors_MatchesTheRangeFilteredRepositorySequence([Range(1, 4)] int numberOfInstruments)
    {
        AssertParityWithTheRepositoryOracle(numberOfInstruments, _minRangeAnchors);
    }

    [Test]
    public void EnumerateCandidates_FromWideMidRangeAnchors_PrunesNothing()
    {
        // arrange: both voices sit >= MaxScaleStepChange diatonic steps from either range edge, so every combination
        // survives and the enumerator must reproduce the repository verbatim.
        var configuration = TestCompositionConfigurations.Get(2);
        var currentChord = BuildChord(configuration, _midRangeAnchors);
        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());
        var oracle = EnumerateWithRepository(configuration, currentChord);

        // act
        var candidates = enumerator.EnumerateCandidates(currentChord).ToList();

        // assert
        candidates.Should().Equal(oracle);
    }

    [Test]
    public void EnumerateCandidates_NeverYieldsAnOutOfRangeNote([Range(1, 4)] int numberOfInstruments)
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments);
        var currentChord = BuildChord(configuration, _maxRangeAnchors);
        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());

        // act
        var candidates = enumerator.EnumerateCandidates(currentChord).ToList();

        // assert
        candidates.Should().NotBeEmpty();
        candidates.SelectMany(static candidate => candidate.Chord.Notes)
            .Should()
            .OnlyContain(note => configuration.IsNoteInInstrumentRange(note.Instrument, note.Raw));
    }

    [Test]
    public void EnumerateCandidates_WithAPartiallyVoicedChord_MatchesTheRangeFilteredRepositorySequence()
    {
        // arrange: only two of the quartet's four voices sound (a staggered fugal exposition); the absent voices'
        // choices must still vary the ChordChoice while contributing no note, exactly like the repository path.
        var configuration = TestCompositionConfigurations.Get(4);
        var currentChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C5, configuration.DefaultNoteTimeSpan),
            new BaroquenNote(Instrument.Two, Notes.G4, configuration.DefaultNoteTimeSpan)
        ]);

        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());
        var oracle = EnumerateWithRepository(configuration, currentChord)
            .Where(candidate => AllNotesAreInRange(configuration, candidate.Chord))
            .ToList();

        // act
        var candidates = enumerator.EnumerateCandidates(currentChord).ToList();

        // assert
        candidates.Should().Equal(oracle);
    }

    [Test]
    public void EnumerateCandidates_WhenAPresentVoiceHasNoViableChoices_YieldsNothing()
    {
        // arrange: C7 sits more than MaxScaleStepChange diatonic steps above Instrument.One's C6 ceiling, so no choice
        // can bring the voice back in range and the whole domain is empty — matching the repository path, where every
        // candidate would fail the non-bypassable range rule.
        var configuration = TestCompositionConfigurations.Get(2);
        var currentChord = BuildChord(configuration, [Notes.C7, Notes.G3]);
        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());

        // act
        var candidates = enumerator.EnumerateCandidates(currentChord).ToList();

        // assert
        candidates.Should().BeEmpty();
        EnumerateWithRepository(configuration, currentChord)
            .Where(candidate => AllNotesAreInRange(configuration, candidate.Chord))
            .Should()
            .BeEmpty("the oracle agrees that no candidate survives the range gate");
    }

    [Test]
    public void Constructor_WithNoInstrumentConfigurations_ThrowsArgumentException()
    {
        // arrange: the legacy repository factory failed fast on an instrument-less configuration; the forward-checking
        // path must preserve that contract instead of silently composing empty chords.
        var configuration = TestCompositionConfigurations.Get(0);

        // act
        var act = () => new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());

        // assert
        act.Should().Throw<ArgumentException>().WithParameterName("compositionConfiguration");
    }

    [Test]
    public void EnumerateCandidates_YieldsFreshNoteInstancesPerCandidate()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var currentChord = BuildChord(configuration, _midRangeAnchors);
        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());

        // act: mutate the first candidate's notes as the ornamentation pipeline legitimately would.
        var candidates = enumerator.EnumerateCandidates(currentChord).Take(2).ToList();

        foreach (var note in candidates[0].Chord.Notes)
        {
            note.MusicalTimeSpan = MusicalTimeSpan.ThirtySecond;
        }

        // assert: neither the source chord nor sibling candidates share note instances with the mutated candidate.
        currentChord.Notes.Should().OnlyContain(note => note.MusicalTimeSpan == configuration.DefaultNoteTimeSpan);
        candidates[1].Chord.Notes.Should().OnlyContain(note => note.MusicalTimeSpan == configuration.DefaultNoteTimeSpan);
    }

    private static void AssertParityWithTheRepositoryOracle(int numberOfInstruments, Note[] anchors)
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(numberOfInstruments);
        var currentChord = BuildChord(configuration, anchors);
        var enumerator = new ForwardCheckingChordChoiceEnumerator(configuration, new NoteChoiceGenerator());

        var oracle = EnumerateWithRepository(configuration, currentChord)
            .Where(candidate => AllNotesAreInRange(configuration, candidate.Chord))
            .ToList();

        // act
        var candidates = enumerator.EnumerateCandidates(currentChord).ToList();

        // assert: identical candidates in the identical order — the load-bearing byte-parity property.
        candidates.Should().Equal(oracle);
    }

    /// <summary>
    ///     The brute-force oracle: enumerate the full Cartesian repository and materialize every candidate, exactly as
    ///     the pre-forward-checking <c>CompositionStrategy</c> did.
    /// </summary>
    private static List<(ChordChoice ChordChoice, BaroquenChord Chord)> EnumerateWithRepository(CompositionConfiguration configuration, BaroquenChord currentChord)
    {
        var repository = new ChordChoiceRepositoryFactory(new NoteChoiceGenerator()).Create(configuration);
        var candidates = new List<(ChordChoice, BaroquenChord)>();

        for (BigInteger chordChoiceId = 0; chordChoiceId < repository.Count; ++chordChoiceId)
        {
            var chordChoice = repository.GetChordChoice(chordChoiceId);
            var chord = currentChord.ApplyChordChoice(configuration.Scale, chordChoice, configuration.DefaultNoteTimeSpan);

            candidates.Add((chordChoice, chord));
        }

        return candidates;
    }

    private static bool AllNotesAreInRange(CompositionConfiguration configuration, BaroquenChord chord) =>
        chord.Notes.All(note => configuration.IsNoteInInstrumentRange(note.Instrument, note.Raw));

    private static BaroquenChord BuildChord(CompositionConfiguration configuration, Note[] anchors)
    {
        var instruments = configuration.Instruments.Order().ToList();
        var notes = instruments
            .Select((instrument, index) => new BaroquenNote(instrument, anchors[index], configuration.DefaultNoteTimeSpan))
            .ToList();

        return new BaroquenChord(notes);
    }
}
