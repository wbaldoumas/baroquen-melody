using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Rhythm.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using NoteName = Melanchall.DryWetMidi.MusicTheory.NoteName;
using Notes = Melanchall.DryWetMidi.MusicTheory.Notes;

namespace BaroquenMelody.Library.Tests.Rhythm;

/// <summary>
///     End-to-end anchors for accompaniment textures. Seeded walks differ across operating systems, so
///     feature-on assertions are all-notes invariants (deterministic consequences of the weight-zero gates),
///     all-seed separations, or in-process byte-identity pairs - never per-seed outcome pins on
///     probabilistic signatures. Family membership, not figure density, is the deciding metric: stock
///     decoration already figures most beats, so a rate could not separate texture from baseline. The
///     invariants scope by INSTRUMENT and REGION - the ledger only brackets the body span - never by
///     per-note ledger membership: filtering assertions through the same predicate that drives the gates
///     would make them self-fulfilling, blind to any note the recording missed.
/// </summary>
[TestFixture]
[Category("Composition")]
[Parallelizable(ParallelScope.All)]
internal sealed class TextureCompositionTests
{
    // Everything a recorded figuration-voice note may legally carry besides its family: the unfired case,
    // the sustain pass's stamps, the suspension pass's restamps, and the phraser's cadential trill - the
    // one figure stamped through a bare processor with no input policies, which can select the figuration
    // voice whenever no higher voice sounds the leading tone.
    private static readonly OrnamentationType[] NonFamilyAllowance =
    [
        OrnamentationType.None,
        OrnamentationType.Sustain,
        OrnamentationType.MidSustain,
        OrnamentationType.Suspension,
        OrnamentationType.SuspensionResolution,
        OrnamentationType.Trill
    ];

    [Test]
    public void Compose_WithVoiceRhythmOff_TheTextureIsInert()
    {
        foreach (var seed in Enumerable.Range(1, 2))
        {
            // arrange - the voice-rhythm configuration is the texture's master switch: with it off, the
            // texture flag must change nothing at all
            var texturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.Walking, voiceRhythmEnabled: false), seed));
            var untexturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.None, voiceRhythmEnabled: false), seed));

            // assert
            texturedNotes.Should().Equal(untexturedNotes, $"voice-rhythm-off must render the texture flag inert (seed {seed})");
        }
    }

    [Test]
    public void Compose_AGroundWithAPlan_IsByteIdenticalWhateverTheTexture()
    {
        foreach (var seed in Enumerable.Range(1, 2))
        {
            // arrange - the ground records no texture marks, so a planned ground must render byte-identically
            // whatever the texture configuration says; only the plan-less fallback path takes the texture
            var groundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord);
            var texturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.BrokenChord, groundBassConfiguration: groundBassConfiguration), seed));
            var untexturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.None, groundBassConfiguration: groundBassConfiguration), seed));

            // assert
            texturedNotes.Should().Equal(untexturedNotes, $"a planned ground must ignore the texture entirely (seed {seed})");
        }
    }

    [Test]
    public void Compose_AnUnplannableGroundWithATexture_FallsBackToATexturedFugue()
    {
        // arrange - a seven-semitone bass range cannot host the octave-spanning romanesca, so the planner
        // yields no plan and the ground falls back to the fugal composer - which holds the texture-aware
        // scheduler, so the fallback fugue composes WITH the texture (it is a fugue; the texture belongs)
        var texturedConfiguration = GetUnplannableGroundConfiguration(TextureType.Walking);
        var untexturedConfiguration = GetUnplannableGroundConfiguration(TextureType.None);

        // act
        var texturedNotes = SeededComposition.Notes(SeededComposition.Compose(texturedConfiguration, seed: 1));
        var untexturedNotes = SeededComposition.Notes(SeededComposition.Compose(untexturedConfiguration, seed: 1));

        // assert - the planner's decline pins that the fallback actually engaged (a future bank or range
        // change that lets the romanesca fit would otherwise repoint this test's failure at the texture
        // instead of the planner), and the note streams prove the fallback fugue took the texture
        new GroundBassPlanner(texturedConfiguration, new SeededRandomProvider(1))
            .CreatePlan()
            .Should().BeNull("the romanesca cannot fit a seven-semitone bass range");

        texturedNotes.Should().NotBeEmpty("the fallback fugue must compose successfully with a texture active");
        texturedNotes.Should().NotEqual(untexturedNotes, "the fallback fugue takes the texture, unlike the planned ground");
    }

    [TestCase(TextureType.Walking)]
    [TestCase(TextureType.BrokenChord)]
    public void Compose_WithAFiguralTexture_TheFigurationVoiceCarriesOnlyItsFamily(TextureType texture)
    {
        var family = texture == TextureType.Walking
            ? VoiceRhythmPolicyTransformer.WalkingFigures
            : VoiceRhythmPolicyTransformer.BrokenChordFigures;

        var allowedTypes = family.Concat(NonFamilyAllowance).ToHashSet();
        var seedsWithFamilyFigures = 0;

        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act
            var composerGraph = ComposerGraph.Create(GetConfiguration(texture), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var interiorBodyMeasures = GetInteriorBodyMeasures(composition, composerGraph.Ledger, seed);
            var figurationNotes = NotesOf(interiorBodyMeasures, composerGraph.Configuration.Instruments[^1]).ToList();

            figurationNotes.Should().NotBeEmpty($"the figuration voice sounds throughout the body (seed {seed})");

            // assert - the family-membership invariant: a deterministic consequence of the weight-zero
            // gates and the restatement re-clothing, so it holds for every note on every seed
            foreach (var figurationNote in figurationNotes)
            {
                allowedTypes.Should().Contain(figurationNote.OrnamentationType, $"the figuration voice renders only its family (seed {seed})");
            }

            if (figurationNotes.Exists(note => family.Contains(note.OrnamentationType)))
            {
                seedsWithFamilyFigures++;
            }
        }

        // assert - and the family actually fires (an eligibility sweep, not a per-seed pin)
        seedsWithFamilyFigures.Should().BeGreaterThanOrEqualTo(3, "the family must fire on most seeds at near-certain weights");
    }

    [Test]
    public void Compose_WithABrokenChordTexture_TheArpeggioCellCarriesTheFabricAndTheStaticBounceIsGone()
    {
        var seedsWithArpeggioCells = 0;

        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act
            var composerGraph = ComposerGraph.Create(GetConfiguration(TextureType.BrokenChord), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var interiorBodyMeasures = GetInteriorBodyMeasures(composition, composerGraph.Ledger, seed);
            var figurationNotes = NotesOf(interiorBodyMeasures, composerGraph.Configuration.Instruments[^1]).ToList();

            // assert - the evicted static pedals are weight-zero under the texture, so their absence is a
            // deterministic per-seed consequence (the family invariant restated for the correction's core
            // members: the harmonically empty octave bounce may never re-enter the fabric unnoticed)
            figurationNotes.Should().NotContain(
                static note => note.OrnamentationType == OrnamentationType.OctavePedal || note.OrnamentationType == OrnamentationType.UpperOctavePedal,
                $"the static octave bounce is evicted from the broken-chord family (seed {seed})");

            if (figurationNotes.Exists(static note => note.OrnamentationType == OrnamentationType.Arpeggio))
            {
                seedsWithArpeggioCells++;
            }
        }

        // assert - and the chord-tone cell actually carries the fabric (an eligibility sweep, not a
        // per-seed pin: seeded walks differ across operating systems)
        seedsWithArpeggioCells.Should().BeGreaterThanOrEqualTo(3, "the arpeggio's broad degree-gated eligibility must reach most seeds' fabrics");
    }

    [TestCase(TextureType.Walking, 3)]
    [TestCase(TextureType.BrokenChord, 3)]
    [TestCase(TextureType.Walking, 4)]
    [TestCase(TextureType.BrokenChord, 4)]
    public void Compose_WithAFiguralTexture_ThePadsBreatheGentlyAndCarryNothingElse(TextureType texture, int voiceCount)
    {
        var allowedTypes = VoiceRhythmPolicyTransformer.PadGentleFigures.Concat(NonFamilyAllowance).ToHashSet();
        var seedsWithBreathingPads = 0;

        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act - both voice counts matter: 4 voices assign TWO pads (the configuration the
            // listen gate found doubly static), and the pad instruments are derived from the scheduler's
            // own role answers rather than a list index, so a register tiebreak can never silently point
            // these assertions at the wrong voice
            var composerGraph = ComposerGraph.Create(GetConfiguration(texture, voiceCount: voiceCount), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);
            var voiceRhythmScheduler = new VoiceRhythmScheduler(composerGraph.Configuration);

            var padInstruments = composerGraph.Configuration.Instruments
                .Where(instrument => voiceRhythmScheduler.TryGetTextureRole(instrument, out var textureRole) && textureRole == TextureRole.Pad)
                .ToList();

            padInstruments.Should().HaveCount(voiceCount - 2, "every voice between the melody and the figuration is a pad");

            var interiorBodyMeasures = GetInteriorBodyMeasures(composition, composerGraph.Ledger, seed);

            foreach (var padInstrument in padInstruments)
            {
                var padNotes = NotesOf(interiorBodyMeasures, padInstrument).ToList();

                padNotes.Should().NotBeEmpty($"the pad voice sounds throughout the body (seed {seed})");

                // assert - the gentle-only invariant: every figure gate except the gentle pair is
                // weight-zero on pads, so anything else appearing here is a deterministic failure on
                // every seed, for every pad
                foreach (var padNote in padNotes)
                {
                    allowedTypes.Should().Contain(padNote.OrnamentationType, $"a pad may breathe only through the gentle figures (seed {seed})");
                }

                if (padNotes.Exists(static note => VoiceRhythmPolicyTransformer.PadGentleFigures.Contains(note.OrnamentationType)))
                {
                    seedsWithBreathingPads++;
                }
            }
        }

        // assert - and the breathing actually happens (an eligibility sweep, not a per-seed pin: a
        // 24-seed census at HALF the shipped weight measured a per-seed floor of one gentle figure and
        // a mean near four on the single 3-voice pad, so a zero-breath seed is already rare there and
        // rarer at the shipped weight)
        seedsWithBreathingPads.Should().BeGreaterThanOrEqualTo(3, "the pads must breathe on most seeds");
    }

    [Test]
    public void Compose_WithAChordalTexture_TheAccompanimentStaysFigureFreeWhileTheMelodyFigures()
    {
        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act
            var composerGraph = ComposerGraph.Create(GetConfiguration(TextureType.Chordal), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var interiorBodyMeasures = GetInteriorBodyMeasures(composition, composerGraph.Ledger, seed);
            var melodyInstrument = composerGraph.Configuration.Instruments[0];

            var accompanimentNotes = interiorBodyMeasures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .Where(note => note.Instrument != melodyInstrument)
                .ToList();

            accompanimentNotes.Should().NotBeEmpty($"the accompaniment sounds throughout the body (seed {seed})");

            // assert - Chordal's empty family silences every accompaniment gate, so nothing but the sustain
            // and suspension stamps (and the phraser's direct trill) may appear, and only the trill carries
            // sub-notes
            foreach (var accompanimentNote in accompanimentNotes)
            {
                NonFamilyAllowance.Should().Contain(accompanimentNote.OrnamentationType, $"a chordal accompaniment carries no figures (seed {seed})");

                if (accompanimentNote.OrnamentationType != OrnamentationType.Trill)
                {
                    accompanimentNote.Ornamentations.Should().BeEmpty($"a chordal accompaniment note carries no sub-notes (seed {seed})");
                }
            }

            NotesOf(interiorBodyMeasures, melodyInstrument)
                .Any(static note => note.Ornamentations.Count > 0)
                .Should().BeTrue($"the melody still figures over the chordal accompaniment (seed {seed})");
        }
    }

    [Test]
    public void Compose_WithATexture_TheMelodyRendersAboveTheAccompaniment()
    {
        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act
            var composerGraph = ComposerGraph.Create(GetConfiguration(TextureType.Walking), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var interiorBodyMeasures = GetInteriorBodyMeasures(composition, composerGraph.Ledger, seed);
            var melodyInstrument = composerGraph.Configuration.Instruments[0];

            var melodyVelocities = NotesOf(interiorBodyMeasures, melodyInstrument).Select(static note => (int)note.Velocity).ToList();
            var accompanimentVelocities = interiorBodyMeasures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .Where(note => note.Instrument != melodyInstrument)
                .Select(static note => (int)note.Velocity)
                .ToList();

            // assert - the prominence offset separates the means on every seed (its magnitude is pinned at
            // the unit level, where the dynamics walk is held inert)
            melodyVelocities.Should().NotBeEmpty($"the melody sounds throughout the body (seed {seed})");
            accompanimentVelocities.Should().NotBeEmpty($"the accompaniment sounds throughout the body (seed {seed})");
            melodyVelocities.Average().Should().BeGreaterThan(accompanimentVelocities.Average(), $"the melody renders above the accompaniment (seed {seed})");
        }
    }

    [Test]
    public void Compose_WithATexture_RendersDifferentlyFromNoTexture()
    {
        foreach (var seed in Enumerable.Range(1, 2))
        {
            // act
            var texturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.Chordal), seed));
            var untexturedNotes = SeededComposition.Notes(SeededComposition.Compose(GetConfiguration(TextureType.None), seed));

            // assert
            texturedNotes.Should().NotEqual(untexturedNotes, $"an active texture must change the rendered composition (seed {seed})");
        }
    }

    // The ledger's one legitimate test role: BRACKETING the body. The recorded measures delimit the span
    // between the unrecorded exposition prefix and the unrecorded ending suffix; within the bracket the
    // assertions run over EVERY note of the scoped instrument, recorded or not, so a recording gap is a
    // failure rather than an exemption. The last recorded measure is excluded because the ending splices a
    // fresh stock-decorated cadential beat into it; everything before it is body material - walked
    // originals and restatement copies alike, which is asserted directly: a static texture must record
    // every interior body measure.
    private static List<Measure> GetInteriorBodyMeasures(Composition composition, VoiceRhythmLedger ledger, int seed)
    {
        var recordedMeasureIndices = composition.Measures
            .Select((measure, index) => (Measure: measure, Index: index))
            .Where(pair => pair.Measure.Beats.Any(beat => beat.Chord.Notes.Any(note => ledger.IsHeldNote(note) || ledger.IsFloridNote(note) || ledger.IsTextureFigurationNote(note))))
            .Select(static pair => pair.Index)
            .ToList();

        recordedMeasureIndices.Should().NotBeEmpty($"an active texture must record the body (seed {seed})");

        var interiorBodyMeasures = composition.Measures
            .Take(recordedMeasureIndices[^1])
            .Skip(recordedMeasureIndices[0])
            .ToList();

        foreach (var measure in interiorBodyMeasures)
        {
            measure.Beats
                .Any(beat => beat.Chord.Notes.Any(note => ledger.IsHeldNote(note) || ledger.IsFloridNote(note) || ledger.IsTextureFigurationNote(note)))
                .Should().BeTrue($"a static texture records every interior body measure - restatement copies included (seed {seed})");
        }

        return interiorBodyMeasures;
    }

    private static IEnumerable<BaroquenNote> NotesOf(IEnumerable<Measure> measures, Instrument instrument) => measures
        .SelectMany(static measure => measure.Beats)
        .SelectMany(static beat => beat.Chord.Notes)
        .Where(note => note.Instrument == instrument);

    // Production-shaped default ranges rather than the narrow test ranges: the broken-chord family's octave
    // figures need the octave headroom the redesigned defaults guarantee, and the listening artifacts
    // should mirror what the app renders. Constructed fresh - InstrumentConfigurations feeds property
    // initializers a with-clone would leave stale.
    private static CompositionConfiguration GetConfiguration(
        TextureType texture,
        bool voiceRhythmEnabled = true,
        GroundBassConfiguration? groundBassConfiguration = null,
        int voiceCount = 3) => new(
        BuildInstrumentConfigurations(voiceCount),
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 25,
        ShuffleOrnamentationProcessors: false,
        GroundBassConfiguration: groundBassConfiguration,
        VoiceRhythmConfiguration: new VoiceRhythmConfiguration(voiceRhythmEnabled),
        TextureConfiguration: new TextureConfiguration(texture));

    private static HashSet<InstrumentConfiguration> BuildInstrumentConfigurations(int voiceCount)
    {
        var instrumentConfigurations = new HashSet<InstrumentConfiguration>
        {
            InstrumentConfiguration.DefaultConfigurations[Instrument.One],
            InstrumentConfiguration.DefaultConfigurations[Instrument.Two],
            InstrumentConfiguration.DefaultConfigurations[Instrument.Three]
        };

        if (voiceCount == 4)
        {
            var defaultFour = InstrumentConfiguration.DefaultConfigurations[Instrument.Four];

            instrumentConfigurations.Add(new InstrumentConfiguration(
                Instrument.Four,
                defaultFour.MinNote,
                defaultFour.MaxNote,
                defaultFour.MinVelocity,
                defaultFour.MaxVelocity,
                defaultFour.MidiProgram,
                ConfigurationStatus.Enabled));
        }

        return instrumentConfigurations;
    }

    private static CompositionConfiguration GetUnplannableGroundConfiguration(TextureType texture) => new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C4, Notes.C6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Two, Notes.G2, Notes.G4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Three, Notes.C2, Notes.G2, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        },
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 25,
        ShuffleOrnamentationProcessors: false,
        GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca),
        TextureConfiguration: new TextureConfiguration(texture));
}
