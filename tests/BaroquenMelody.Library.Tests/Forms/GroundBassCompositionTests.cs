using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Mode = BaroquenMelody.Library.MusicTheory.Enums.Mode;

namespace BaroquenMelody.Library.Tests.Forms;

/// <summary>
///     End-to-end coverage for the ground bass form, asserted over the rendered MIDI. The bass voice is
///     isolated as the track sounding at tick zero: the form opens with the ground alone, so exactly one
///     track may sound there - itself one of the invariants under test.
/// </summary>
/// <remarks>
///     The ground-cycle property tolerates a licensed semitone raise per position (the tonicization pass may
///     raise a bass note carrying a raising triad's third at a statement seam - classic varied-ground
///     chromaticism) and compares deduplicated consecutive pitches, which makes it robust to sustain merges
///     and suspension time-shifts, both order-preserving. Assertions here are structural properties that hold
///     for every seed on every platform; nothing pins a per-seed outcome.
/// </remarks>
[TestFixture]
[Category("Composition")]
internal sealed class GroundBassCompositionTests
{
    private const int TicksPerQuarterNote = 96;

    [Test]
    public void Compose_WithTheGroundBassForm_IsDeterministicUnderASeed() => Gen.Int.Sample(
        seed =>
        {
            var configuration = GetGroundBassConfiguration(3, 10);

            var first = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));
            var second = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            first.Should().NotBeEmpty();
            first.Should().Equal(second, "the plan draw, the pinned walk, and every pass must flow through the seeded random provider");
        },
        iter: 3
    );

    [Test]
    public void Compose_WithTheGroundBassForm_OpensWithTheBassAloneStatingAGroundFromTheBank()
    {
        var configuration = GetGroundBassConfiguration(3, 10);

        for (var seed = 1; seed <= 3; ++seed)
        {
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: the opening bass pitches must state a bank pattern exactly");

            var statementTicks = (long)ground.RenderedPattern!.Count * GroundBassPlan.SlotsPerGroundNote * SlotTicks(configuration);

            ground.OtherVoiceEntryTimes.Should().OnlyContain(
                entryTime => entryTime >= statementTicks,
                $"seed {seed}: no upper voice may enter before the announcement completes");
        }
    }

    [Test]
    public void Compose_WithTheGroundBassForm_ClosesWithAWholeNoteFinalChordInEveryVoice()
    {
        // The close is crafted directly (a whole-note tonic arrival plus a trailing rest), so every voice's
        // last sounding note must be that whole note, ending together: no later pass may rewrite the crafted
        // close - neither truncating it nor absorbing it into an extended predecessor that outlives it.
        var configuration = GetGroundBassConfiguration(3, 10);
        var wholeNoteTicks = 4L * TicksPerQuarterNote;

        for (var seed = 1; seed <= 3; ++seed)
        {
            var lastNotes = SeededComposition.Compose(configuration, seed).MidiFile.GetTrackChunks()
                .Select(static chunk => chunk.GetNotes().ToList())
                .Where(static notes => notes.Count > 0)
                .Select(static notes => notes[^1])
                .ToList();

            var closeTime = lastNotes.Max(static note => note.EndTime);

            lastNotes.Should().OnlyContain(
                note => note.EndTime == closeTime && note.Length == wholeNoteTicks,
                $"seed {seed}: every voice's last sounding note must be the crafted whole-note final chord");
        }
    }

    [Test]
    public void Compose_WithAConfiguredPattern_StatesExactlyThatPattern()
    {
        // arrange: the free draw could state any feasible ground, so this pins the cadential ground and
        // demands the announcement render it - the plan must honor the configuration through the whole
        // pipeline, for every seed.
        var configuration = GetGroundBassConfiguration(3, 10, GroundBass.CadentialGround);
        var cadentialGround = GroundBassPattern.Bank.Single(static pattern => pattern.Identifier == GroundBass.CadentialGround);
        var scalePitches = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToList();

        for (var seed = 1; seed <= 3; ++seed)
        {
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: the opening bass pitches must state a bank pattern exactly");

            var anchorIndex = scalePitches.IndexOf(ground.RenderedPattern![0]);
            var expectedPattern = cadentialGround.ScaleStepOffsets.Select(offset => scalePitches[anchorIndex + offset]).ToList();

            ground.RenderedPattern.Should().Equal(expectedPattern, $"seed {seed}: the configured cadential ground must be the stated pattern");
        }
    }

    [Test]
    public void Compose_WithTheGroundBassForm_RendersEveryPlannedStatementModuloLicensedRaises()
    {
        // A licensed raise always lands on a ground note's HELD slot (the tonicization pass's dominant
        // sits at an odd slot in both of its pair shapes), so the bass renders the natural onset and then
        // its raised form - the classic chromatic varied-ground bass (e.g. G, G#, A). The walk below
        // consumes the PLAN's expected bass line note by note, tolerating a raised insertion right after
        // its natural. The plan is reproduced exactly by a fresh planner over a fresh seeded provider,
        // because the pattern draw is the composition stream's first draw - so the expectation covers the
        // modulating middle statements at the relative key's pitch level, whichever pattern the seed draws.
        var configuration = GetGroundBassConfiguration(3, 25);

        for (var seed = 1; seed <= 5; ++seed)
        {
            var plan = new GroundBassPlanner(configuration, new SeededRandomProvider(seed)).CreatePlan();

            plan.Should().NotBeNull($"seed {seed}: the test-shaped bass hosts the whole bank");

            var expectedPitches = Enumerable.Range(0, plan!.StatementCount)
                .SelectMany(statementIndex => plan.SectionForStatement(statementIndex).BassNotes)
                .Select(static note => (int)note.NoteNumber)
                .ToList();
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: the opening statement must state a bank pattern exactly");
            ground.RenderedPattern.Should().Equal(
                plan.BassNotes.Select(static note => (int)note.NoteNumber),
                $"seed {seed}: the announcement must state the home rendering the plan chose");

            var expectedIndex = 0;

            for (var pitchIndex = 0; pitchIndex < ground.DedupedBassPitches.Count - 1; ++pitchIndex)
            {
                var observedPitch = ground.DedupedBassPitches[pitchIndex];

                if (expectedIndex < expectedPitches.Count && observedPitch == expectedPitches[expectedIndex])
                {
                    ++expectedIndex;

                    continue;
                }

                var isLicensedRaiseOfPreviousNote = expectedIndex > 0 && observedPitch == expectedPitches[expectedIndex - 1] + 1;

                isLicensedRaiseOfPreviousNote.Should().BeTrue($"seed {seed}: position {pitchIndex} (pitch {observedPitch}) must carry the planned ground or trail its natural as a licensed raise");
            }

            expectedIndex.Should().Be(expectedPitches.Count, $"seed {seed}: every planned statement must render in full at its section's pitch level");
            (ground.DedupedBassPitches[^1] % 12).Should().Be(expectedPitches[0] % 12, $"seed {seed}: the close must land on the home tonic");
        }
    }

    [TestCase(NoteName.C, Mode.Ionian, TestName = "Compose_WithModulationOn_JourneysThroughTheRelativeMinorAndReturns")]
    [TestCase(NoteName.A, Mode.Aeolian, TestName = "Compose_WithModulationOn_JourneysThroughTheRelativeMajorAndReturns")]
    public void Compose_WithModulationOn_JourneysThroughTheRelativeKeyAndReturns(NoteName tonic, Mode mode)
    {
        // arrange: the pinned tetrachord modulates deterministically over the test-shaped bass (the plan
        // is draw-independent past the pattern), journeying to the relative key in both directions. The
        // relative rendering's two distinctive pitches sit outside the home rendering entirely, so their
        // presence in the bass is the journey made audible - and both directions drive the configurator's
        // relative-stack construction end to end.
        var configuration = TestCompositionConfigurations.Get(3, 25, tonic, mode) with
        {
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };

        for (var seed = 1; seed <= 3; ++seed)
        {
            var plan = new GroundBassPlanner(configuration, new SeededRandomProvider(seed)).CreatePlan();

            plan.Should().NotBeNull();
            plan!.Sections.Should().HaveCount(3, $"seed {seed}: the pinned tetrachord must plan the relative journey");

            var homePitches = plan.BassNotes.Select(static note => (int)note.NoteNumber).ToList();
            var distinctiveForeignPitches = plan.Sections[1].BassNotes
                .Select(static note => (int)note.NoteNumber)
                .Where(pitch => !homePitches.Contains(pitch))
                .ToList();
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            distinctiveForeignPitches.Should().HaveCount(2, "sanity: the relative tetrachord shares two pitches with home and brings two of its own");

            foreach (var distinctiveForeignPitch in distinctiveForeignPitches)
            {
                ground.DedupedBassPitches.Should().Contain(distinctiveForeignPitch, $"seed {seed}: the relative statements must sound pitches the home ground never touches");
            }

            (ground.DedupedBassPitches[^1] % 12).Should().Be(homePitches[0] % 12, $"seed {seed}: the journey must return home for the close");
        }
    }

    [Test]
    public void Compose_WithModulationOff_StatesTheHomeGroundInEveryStatement()
    {
        // The pre-modulation contract, preserved verbatim behind the toggle: every statement renders the
        // home cycle, whichever pattern the seed draws.
        var configuration = GetGroundBassConfiguration(3, 25, pattern: null, modulate: false);

        for (var seed = 1; seed <= 3; ++seed)
        {
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: the opening statement must state a bank pattern exactly");

            var renderedPattern = ground.RenderedPattern!;
            var patternLength = renderedPattern.Count;
            var measuresPerStatement = patternLength * GroundBassPlan.SlotsPerGroundNote / configuration.BeatsPerMeasure;
            var expectedStatements = Math.Max(2, (configuration.MinimumMeasures + measuresPerStatement - 1) / measuresPerStatement);
            var expectedIndex = 0;

            for (var pitchIndex = 0; pitchIndex < ground.DedupedBassPitches.Count - 1; ++pitchIndex)
            {
                var observedPitch = ground.DedupedBassPitches[pitchIndex];

                if (observedPitch == renderedPattern[expectedIndex % patternLength])
                {
                    ++expectedIndex;

                    continue;
                }

                var isLicensedRaiseOfPreviousNote = expectedIndex > 0 && observedPitch == renderedPattern[(expectedIndex - 1) % patternLength] + 1;

                isLicensedRaiseOfPreviousNote.Should().BeTrue($"seed {seed}: position {pitchIndex} (pitch {observedPitch}) must carry the home ground or trail its natural as a licensed raise");
            }

            (expectedIndex % patternLength).Should().Be(0, $"seed {seed}: the bass line must complete whole statements before the final tonic");
            (expectedIndex / patternLength).Should().Be(expectedStatements, $"seed {seed}: every planned statement must render");
            (ground.DedupedBassPitches[^1] % 12).Should().Be(renderedPattern[0] % 12, $"seed {seed}: the close must land on the tonic");
        }
    }

    [Test]
    public void Compose_InAnUnliftedMode_TheModulateToggleIsInert()
    {
        // Dorian sits outside the lifted modes, so the toggle must not perturb a single byte: the planner
        // draws identically, the plan is home-only either way, and no seam machinery ever engages.
        var baseConfiguration = TestCompositionConfigurations.Get(3, 25, NoteName.C, Mode.Dorian) with { ShuffleOrnamentationProcessors = false };
        var modulateOnConfiguration = baseConfiguration with { GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, Pattern: null, Modulate: true) };
        var modulateOffConfiguration = baseConfiguration with { GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, Pattern: null, Modulate: false) };

        for (var seed = 1; seed <= 2; ++seed)
        {
            var modulateOnNotes = SeededComposition.Notes(SeededComposition.Compose(modulateOnConfiguration, seed));
            var modulateOffNotes = SeededComposition.Notes(SeededComposition.Compose(modulateOffConfiguration, seed));

            modulateOnNotes.Should().Equal(modulateOffNotes, $"seed {seed}: an unlifted mode must compose identically whatever the toggle");
        }
    }

    [Test]
    public void Compose_WithTheGroundBassForm_HonorsTheConfiguredMinimumMeasures()
    {
        var configuration = GetGroundBassConfiguration(3, 25);

        for (var seed = 1; seed <= 3; ++seed)
        {
            var notes = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));
            var minimumTicks = (long)configuration.MinimumMeasures * configuration.BeatsPerMeasure * SlotTicks(configuration);

            notes.Max(static note => note.Time + note.Length).Should().BeGreaterThanOrEqualTo(minimumTicks, $"seed {seed}: the statements must cover the configured minimum measures");
        }
    }

    [Test]
    public void Compose_WithTheFormExplicitlyDisabled_IsByteIdenticalToTheAbsentConfiguration()
    {
        // The form branch selects a composer before any random draw, so an explicitly disabled
        // configuration must reproduce the legacy pipeline exactly.
        var absentConfiguration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };
        var disabledConfiguration = absentConfiguration with { GroundBassConfiguration = new GroundBassConfiguration(Enabled: false) };

        for (var seed = 1; seed <= 3; ++seed)
        {
            var absentNotes = SeededComposition.Notes(SeededComposition.Compose(absentConfiguration, seed));
            var disabledNotes = SeededComposition.Notes(SeededComposition.Compose(disabledConfiguration, seed));

            disabledNotes.Should().Equal(absentNotes, $"seed {seed}: a disabled form must not perturb the standard pipeline");
        }
    }

    [Test]
    public void Compose_WithTheGroundBassForm_ProducesDifferentMusicThanTheStandardForm()
    {
        var standardConfiguration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };
        var groundConfiguration = GetGroundBassConfiguration(3, 10);

        var anySeedDiffers = Enumerable.Range(1, 5).Any(seed =>
        {
            var standardNotes = SeededComposition.Notes(SeededComposition.Compose(standardConfiguration, seed));
            var groundNotes = SeededComposition.Notes(SeededComposition.Compose(groundConfiguration, seed));

            return !standardNotes.SequenceEqual(groundNotes);
        });

        anySeedDiffers.Should().BeTrue("the ground bass form must change the rendered composition end-to-end");
    }

    [Test]
    public void Compose_WithTheGroundBassFormInTripleMeter_TreadsTheGroundThroughWholeBars()
    {
        var configuration = TestCompositionConfigurations.Get(3, 10) with
        {
            Meter = Meter.ThreeFour,
            DefaultNoteTimeSpan = Meter.ThreeFour.DefaultMusicalTimeSpan(),
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true)
        };

        for (var seed = 1; seed <= 2; ++seed)
        {
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: the triple-meter ground must still state a bank pattern");
            ground.DedupedBassPitches.Count.Should().BeGreaterThan(ground.RenderedPattern!.Count, $"seed {seed}: the ground must repeat beyond its announcement");
        }
    }

    [Test]
    public void Compose_WithTheGroundBassFormAndASingleVoice_ComposesTheGroundAlone()
    {
        var configuration = GetGroundBassConfiguration(1, 10);

        for (var seed = 1; seed <= 2; ++seed)
        {
            var ground = AnalyzeGround(SeededComposition.Compose(configuration, seed), configuration);

            ground.RenderedPattern.Should().NotBeNull($"seed {seed}: a single voice still states the ground");
            ground.OtherVoiceEntryTimes.Should().BeEmpty($"seed {seed}: there is no other voice to enter");
        }
    }

    private static CompositionConfiguration GetGroundBassConfiguration(int numberOfInstruments, int minimumMeasures, GroundBass? pattern = null, bool modulate = true) =>
        TestCompositionConfigurations.Get(numberOfInstruments, minimumMeasures) with
        {
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, pattern, modulate)
        };

    private static long SlotTicks(CompositionConfiguration configuration) =>
        4L * TicksPerQuarterNote * configuration.DefaultNoteTimeSpan.Numerator / configuration.DefaultNoteTimeSpan.Denominator;

    /// <summary>
    ///     Isolates the bass voice as the single track sounding at tick zero (asserting the solo opening on
    ///     the way), deduplicates its consecutive pitches, and identifies the announced bank pattern from the
    ///     opening statement, which no pass may alter.
    /// </summary>
    private static GroundAnalysis AnalyzeGround(MidiFileComposition composition, CompositionConfiguration configuration)
    {
        var trackNotes = composition.MidiFile.GetTrackChunks()
            .Select(static chunk => chunk.GetNotes().ToList())
            .Where(static notes => notes.Count > 0)
            .ToList();

        var bassTracks = trackNotes.Where(static notes => notes[0].Time == 0).ToList();

        bassTracks.Should().ContainSingle("the ground announces itself alone, so exactly one track may sound at tick zero");

        var dedupedBassPitches = new List<int>();

        foreach (var pitch in bassTracks[0].Select(static note => (int)note.NoteNumber))
        {
            if (dedupedBassPitches.Count == 0 || dedupedBassPitches[^1] != pitch)
            {
                dedupedBassPitches.Add(pitch);
            }
        }

        var renderedPattern = IdentifyPattern(dedupedBassPitches, configuration);
        var otherVoiceEntryTimes = trackNotes
            .Where(static notes => notes[0].Time != 0)
            .Select(static notes => notes[0].Time)
            .ToList();

        return new GroundAnalysis(dedupedBassPitches, renderedPattern, otherVoiceEntryTimes);
    }

    private static List<int>? IdentifyPattern(List<int> dedupedBassPitches, CompositionConfiguration configuration)
    {
        var scalePitches = configuration.Scale.GetNotes().Select(static note => (int)note.NoteNumber).ToList();
        var anchorIndex = scalePitches.IndexOf(dedupedBassPitches[0]);

        if (anchorIndex < 0)
        {
            return null;
        }

        foreach (var pattern in GroundBassPattern.Bank)
        {
            var rendered = pattern.ScaleStepOffsets.Select(offset => scalePitches[anchorIndex + offset]).ToList();

            if (dedupedBassPitches.Count >= rendered.Count && dedupedBassPitches.Take(rendered.Count).SequenceEqual(rendered))
            {
                return rendered;
            }
        }

        return null;
    }

    private sealed record GroundAnalysis(List<int> DedupedBassPitches, List<int>? RenderedPattern, List<long> OtherVoiceEntryTimes);
}
