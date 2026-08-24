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
[Category("Composition")]
internal sealed class GroundBassDivisionCompositionTests
{
    private const long TicksPerStatement = 1536;

    private const long SixteenthTicks = 24;

    private const long TiedPairTicks = 384;

    private const int StatementCount = 13;

    // The planner's draw-free formula for thirteen statements: about a third go foreign, centrally
    // placed - statements 4 through 7. Drift in either constant fails the close-position guard below.
    private const int FirstForeignStatement = 4;

    private const int LastForeignStatement = 7;

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
    public void Compose_WithDivisions_TheStatementOnsetTreadAlwaysTies()
    {
        // arrange - the statement-level half of the tread claim, and the cross-key ledger fact with it,
        // pinned per seed because it is a deterministic consequence chain. At suspension probability 100
        // the pass reshapes every stepwise interior boundary (preparations extend, resolutions re-attack
        // late - those re-strikes are the suspension idiom, not broken treads), but the statement-initial
        // pair it can never touch: the pair spans its own unchanging harmony, and the seam into it is the
        // ground's returning leap, never suspension-eligible. Held notes are ornament-silenced (weight 0),
        // so that whole pair is always sustain-eligible and the certain gate (weight 100) ties it -
        // however the walk falls on any host, every statement opens with a single tone longer than a half
        // note, where the stock gate (weight 80) drops one opening in five back to a re-struck pair. The
        // foreign block (statements 4-7) rides the same pin as the shared-ledger tripwire: a relative
        // stack with its own ledger would leave its statement openings at stock odds.
        foreach (var seed in Enumerable.Range(1, 8))
        {
            // act
            var bassNotes = GetBassTrackNotes(SeededComposition.Compose(GetConfiguration(divisions: true), seed));

            // the close-position guard: the hand-derived StatementCount must track the planner, or every
            // statement window in this fixture silently measures the wrong music
            bassNotes.Max(static note => note.Time).Should().BeInRange(
                StatementCount * TicksPerStatement,
                ((StatementCount + 1) * TicksPerStatement) - 1,
                $"the close must begin exactly after statement {StatementCount - 1} for seed {seed}");

            // assert
            UnmergedStatementOnsetCount(bassNotes, 1, StatementCount - 1).Should().Be(0, $"the held tread must tie every statement's opening pair for seed {seed}");
            UnmergedStatementOnsetCount(bassNotes, FirstForeignStatement, LastForeignStatement).Should().Be(0, $"the relative key's engine must tie the tread from the one shared ledger for seed {seed}");
        }
    }

    [Test]
    public void Compose_WithDivisions_TheSixteenthLiftGrowsTowardTheClose()
    {
        // arrange - the escalation signature, isolated from the baseline's own statement-to-statement
        // structure: against the SAME seed's divisions-off render, the sixteenth-tier lift the feature
        // causes in the last third of statements must exceed its lift in the first third (where calm
        // intensities suppress the tier at or below stock). Figure realization is probabilistic and
        // cross-OS walks differ, so this is a threshold sweep, never a per-seed pin. The 6/8 floor is a
        // margin, not the observed value: the windows' intensities separate 30-60 against 110-140, a
        // measured tier-ratio spread of roughly 0.85x against 1.5-1.9x, and it also discriminates the
        // uniform-scaling failure the probe actually caught, where the lift is flat and the comparison
        // degenerates to a coin flip.
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
        // arrange - the terrace signature, isolated from the baseline's own dynamics shape the way the
        // sixteenth test isolates from its statement curve (the upper voices' statement-1 entry seeds
        // below the ceiling, so even an unterraced render trends louder toward the close): against the
        // SAME seed's divisions-off render, the velocity the feature removes from statement 1 (five
        // steps) must exceed what it removes from the final statement (none). Figure divergence keeps the
        // two renders' dynamics walks from aligning exactly, so this is a threshold sweep - but the
        // five-step signal dwarfs the walk's +/-1 wander, leaving the 6/8 floor generous.
        var seedsWithTerrace = 0;

        foreach (var seed in Enumerable.Range(1, 8))
        {
            // act
            var divisionsOnNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: true), seed));
            var divisionsOffNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(divisions: false), seed));

            var earlyDrop = MeanVelocity(divisionsOnNotes, firstStatement: 1, lastStatement: 1) - MeanVelocity(divisionsOffNotes, firstStatement: 1, lastStatement: 1);
            var finalDrop = MeanVelocity(divisionsOnNotes, firstStatement: StatementCount - 1, lastStatement: StatementCount - 1) - MeanVelocity(divisionsOffNotes, firstStatement: StatementCount - 1, lastStatement: StatementCount - 1);

            if (earlyDrop < finalDrop)
            {
                ++seedsWithTerrace;
            }
        }

        // assert
        seedsWithTerrace.Should().BeGreaterThanOrEqualTo(6, "the opening statement terraces five velocity steps below its unterraced self; the final statement none");
    }

    private static CompositionConfiguration GetConfiguration(bool divisions, bool voiceRhythmEnabled = true) =>
        TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, Pattern: GroundBass.DescendingTetrachord, Modulate: true, Divisions: divisions),
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled),

            // Pinned rather than defaulted: the tread tests' superset argument (identical suspension
            // stamps between the toggles) holds only at probability 100, where every eligible site
            // suspends regardless of the draw's value.
            SuspensionConfiguration = new SuspensionConfiguration(Enabled: true, Probability: 100),
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

    // A statement's opening ground note always attacks exactly at the statement boundary; when its pair
    // is tied the single tone runs at least a whole pair (longer when a suspension preparation extends
    // it), while an unmerged pair leaves the opening attack at a bare half note. Single() throws loudly
    // if the structure ever drifts from one bass attack per statement boundary.
    private static int UnmergedStatementOnsetCount(IReadOnlyList<Note> bassNotes, int firstStatement, int lastStatement) =>
        Enumerable.Range(firstStatement, lastStatement - firstStatement + 1)
            .Select(statement => bassNotes.Single(note => note.Time == statement * TicksPerStatement))
            .Count(static onsetNote => onsetNote.Length < TiedPairTicks);

    private static double MeanVelocity(IReadOnlyList<MidiNoteSnapshot> notes, int firstStatement, int lastStatement)
    {
        var windowNotes = notes
            .Where(note => note.Time >= firstStatement * TicksPerStatement && note.Time < (lastStatement + 1) * TicksPerStatement)
            .ToList();

        return windowNotes.Count == 0 ? 0 : windowNotes.Average(static note => note.Velocity);
    }
}
