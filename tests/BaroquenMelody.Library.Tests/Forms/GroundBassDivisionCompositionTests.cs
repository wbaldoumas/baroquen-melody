using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Forms;

/// <summary>
///     End-to-end anchors for divisions on a ground, composed through the real configurator via
///     <see cref="SeededComposition"/>. Assertion shapes follow the cross-OS rules: deterministic
///     consequences (the announcement's held ties, the terrace's velocity channel, the tread's
///     superset argument) pin per seed; probabilistic escalation signatures use threshold sweeps.
///     The divisions-off ≡ pre-feature purity claim is decided by the dev-time cross-branch hash
///     harness plus the composer's unit-level "scheduler declines ⇒ records nothing" pin — no
///     in-process test can compare against a graph this branch no longer builds.
/// </summary>
[TestFixture]
internal sealed class GroundBassDivisionCompositionTests
{
    private const long TicksPerStatement = 1536;

    private const long SixteenthTicks = 24;

    private const long TiedPairTicks = 384;

    private const int StatementCount = 13;

    [Test]
    public void Compose_WithDivisionsToggled_RendersDifferently()
    {
        foreach (var seed in Enumerable.Range(1, 5))
        {
            // arrange & act
            var divisionsOnNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: true), seed));
            var divisionsOffNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: false), seed));

            // assert - the terrace's deterministic velocity offset guarantees a difference in every seed,
            // independent of how the probabilistic figure divergence falls
            divisionsOnNotes.Should().NotBeEquivalentTo(divisionsOffNotes, $"divisions must change the render for seed {seed}");
        }
    }

    [Test]
    public void Compose_WithVoiceRhythmOff_TheDivisionsFlagIsInert()
    {
        foreach (var seed in Enumerable.Range(1, 3))
        {
            // arrange & act - voice rhythm is the role machinery's master switch: with it off, both sides
            // record nothing on identical stock-gate graphs
            var divisionsOnNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: true, voiceRhythmEnabled: false), seed));
            var divisionsOffNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: false, voiceRhythmEnabled: false), seed));

            // assert
            divisionsOnNotes.Should().Equal(divisionsOffNotes, $"the divisions flag must be inert under a disabled master switch for seed {seed}");
        }
    }

    [Test]
    public void Compose_AFugue_TheDivisionsFlagIsUnread()
    {
        foreach (var seed in Enumerable.Range(1, 2))
        {
            // arrange & act - a tripwire, not the purity anchor: the form branch never builds ground
            // components when the form is off, so this decides only that nothing outside it reads the flag
            var divisionsOnNotes = SeededComposition.Notes(SeededComposition.Compose(GetFugueConfiguration(divisions: true), seed));
            var divisionsOffNotes = SeededComposition.Notes(SeededComposition.Compose(GetFugueConfiguration(divisions: false), seed));

            // assert
            divisionsOnNotes.Should().Equal(divisionsOffNotes, $"a fugue must never read the divisions flag for seed {seed}");
        }
    }

    [Test]
    public void Compose_WithDivisions_TheAnnouncementGroundIsFullySustained()
    {
        foreach (var seed in Enumerable.Range(1, 8))
        {
            // arrange & act
            var composition = SeededComposition.Compose(GetConfiguration(divisions: true), seed);
            var announcementNotes = GetBassTrackNotes(composition).Where(static note => note.Time < TicksPerStatement).ToList();

            // assert - every announcement pair is sustain-eligible (the suspension pass never sees the
            // solo statement) and the held role's gate weight is 100, so the ties are deterministic:
            // four ground notes, each a single two-beat tone
            announcementNotes.Should().HaveCount(4, $"the tetrachord announces four tied ground notes for seed {seed}");
            announcementNotes.Should().OnlyContain(static note => note.Length == TiedPairTicks, $"every announcement pair must tie into one tone for seed {seed}");
        }
    }

    [Test]
    public void Compose_WithDivisions_TheGroundLineNeverGainsRestrikes()
    {
        foreach (var seed in Enumerable.Range(1, 5))
        {
            // arrange & act - the walk and the suspension stamps are identical between the toggles (the
            // walk is untouched and suspension outcomes are draw-value-independent at probability 100),
            // so divisions-on ties a superset of divisions-off's eligible pairs: never more bass attacks
            var divisionsOnBassCount = GetBassTrackNotes(SeededComposition.Compose(GetConfiguration(divisions: true), seed)).Count;
            var divisionsOffBassCount = GetBassTrackNotes(SeededComposition.Compose(GetConfiguration(divisions: false), seed)).Count;

            // assert
            divisionsOnBassCount.Should().BeLessThanOrEqualTo(divisionsOffBassCount, $"held ties can only merge bass attacks, never add them, for seed {seed}");
        }
    }

    [Test]
    public void Compose_WithDivisions_TheSixteenthLiftGrowsTowardTheClose()
    {
        // arrange - the escalation signature, isolated from the baseline's own statement-to-statement
        // structure: against the SAME seed's divisions-off render, the sixteenth-tier lift the feature
        // causes in the last third of statements must exceed its lift in the first third (where calm
        // intensities suppress the tier at or below stock). Figure realization is probabilistic and
        // cross-OS walks differ, so this is a threshold sweep, never a per-seed pin.
        var seedsWithEscalation = 0;

        foreach (var seed in Enumerable.Range(1, 8))
        {
            // act
            var divisionsOnUpperNotes = GetUpperNotes(SeededComposition.Compose(GetConfiguration(divisions: true), seed));
            var divisionsOffUpperNotes = GetUpperNotes(SeededComposition.Compose(GetConfiguration(divisions: false), seed));

            var earlyLift = SixteenthCount(divisionsOnUpperNotes, 1, 4) - SixteenthCount(divisionsOffUpperNotes, 1, 4);
            var lateLift = SixteenthCount(divisionsOnUpperNotes, StatementCount - 4, StatementCount - 1) - SixteenthCount(divisionsOffUpperNotes, StatementCount - 4, StatementCount - 1);

            if (lateLift > earlyLift)
            {
                ++seedsWithEscalation;
            }
        }

        // assert
        seedsWithEscalation.Should().BeGreaterThanOrEqualTo(6, "the calm-to-florid arc must be measurable in most probed seeds");
    }

    [Test]
    public void Compose_WithDivisions_EarlyStatementsAreQuieterThanTheClose()
    {
        // arrange - the terrace's -4 offset against a +/-1 velocity walk is a large signal, but clamping
        // at the range floor can shave it, so this stays a threshold sweep with a generous margin
        var seedsWithTerrace = 0;

        foreach (var seed in Enumerable.Range(1, 8))
        {
            // act
            var composition = SeededComposition.Compose(GetConfiguration(divisions: true), seed);
            var allNotes = SeededComposition.Notes(composition);

            var earlyMeanVelocity = MeanVelocity(allNotes, firstStatement: 1, lastStatement: 1);
            var finalMeanVelocity = MeanVelocity(allNotes, firstStatement: StatementCount - 1, lastStatement: StatementCount - 1);

            if (earlyMeanVelocity < finalMeanVelocity)
            {
                ++seedsWithTerrace;
            }
        }

        // assert
        seedsWithTerrace.Should().BeGreaterThanOrEqualTo(6, "the opening statement terraces four velocity steps below the close");
    }

    private static CompositionConfiguration GetConfiguration(bool divisions, bool voiceRhythmEnabled = true) =>
        TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, Pattern: GroundBass.DescendingTetrachord, Modulate: true, Divisions: divisions),
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled),
            ShuffleOrnamentationProcessors = false
        };

    private static CompositionConfiguration GetFugueConfiguration(bool divisions) =>
        TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: false, Divisions: divisions),
            ShuffleOrnamentationProcessors = false
        };

    private static List<Note> GetBassTrackNotes(MidiFileComposition composition)
    {
        var trackNotes = composition.MidiFile.GetTrackChunks()
            .Select(static chunk => chunk.GetNotes().ToList())
            .Where(static notes => notes.Count > 0)
            .ToList();

        return trackNotes.Single(static notes => notes[0].Time == 0);
    }

    private static List<Note> GetUpperNotes(MidiFileComposition composition) => composition.MidiFile.GetTrackChunks()
        .Select(static chunk => chunk.GetNotes().ToList())
        .Where(static notes => notes.Count > 0 && notes[0].Time != 0)
        .SelectMany(static notes => notes)
        .ToList();

    private static int SixteenthCount(IReadOnlyList<Note> notes, int firstStatement, int lastStatement) => notes
        .Count(note => note.Time >= firstStatement * TicksPerStatement
                       && note.Time < (lastStatement + 1) * TicksPerStatement
                       && note.Length <= SixteenthTicks);

    private static double MeanVelocity(IReadOnlyList<MidiNoteSnapshot> notes, int firstStatement, int lastStatement)
    {
        var windowNotes = notes
            .Where(note => note.Time >= firstStatement * TicksPerStatement && note.Time < (lastStatement + 1) * TicksPerStatement)
            .ToList();

        return windowNotes.Count == 0 ? 0 : windowNotes.Average(static note => note.Velocity);
    }
}
