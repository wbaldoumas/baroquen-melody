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
using NUnit.Framework;

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
    public void Compose_WithTheGroundBassForm_RepeatsTheGroundInEveryStatementModuloLicensedRaises()
    {
        // A licensed raise always lands on a ground note's HELD slot (the tonicization pass's dominant
        // sits at an odd slot in both of its pair shapes), so the bass renders the natural onset and then
        // its raised form - the classic chromatic varied-ground bass (e.g. G, G#, A). The walk below
        // therefore consumes the expected cycle note by note, tolerating a raised insertion right after
        // its natural.
        var configuration = GetGroundBassConfiguration(3, 25);

        for (var seed = 1; seed <= 5; ++seed)
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

                isLicensedRaiseOfPreviousNote.Should().BeTrue($"seed {seed}: position {pitchIndex} (pitch {observedPitch}) must carry the ground or trail its natural as a licensed raise");
            }

            (expectedIndex % patternLength).Should().Be(0, $"seed {seed}: the bass line must complete whole statements before the final tonic");
            (expectedIndex / patternLength).Should().Be(expectedStatements, $"seed {seed}: every planned statement must render");
            (ground.DedupedBassPitches[^1] % 12).Should().Be(renderedPattern[0] % 12, $"seed {seed}: the close must land on the tonic");
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

    private static CompositionConfiguration GetGroundBassConfiguration(int numberOfInstruments, int minimumMeasures, GroundBass? pattern = null) =>
        TestCompositionConfigurations.Get(numberOfInstruments, minimumMeasures) with
        {
            ShuffleOrnamentationProcessors = false,
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, pattern)
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
