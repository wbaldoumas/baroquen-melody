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
///     all-seed separations with wide margins, or in-process byte-identity pairs - never per-seed outcome
///     pins on probabilistic signatures. Family membership, not figure density, is the deciding metric:
///     stock decoration already figures most beats, so a rate could not separate texture from baseline.
/// </summary>
[TestFixture]
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

        // assert
        texturedNotes.Should().NotBeEmpty("the fallback fugue must compose successfully with a texture active");
        texturedNotes.Should().NotBeEquivalentTo(untexturedNotes, "the fallback fugue takes the texture, unlike the planned ground");
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
            // arrange & act - the hand-wired graph exposes the ledger, and recorded notes are identified by
            // QUERYING it, never positionally: phrasing inserts copies that shift measure indices
            var composerGraph = ComposerGraph.Create(GetConfiguration(texture), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var figurationNotes = composition.Measures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .Where(composerGraph.Ledger.IsTextureFigurationNote)
                .ToList();

            figurationNotes.Should().NotBeEmpty($"an active texture must record its figuration voice (seed {seed})");

            // assert - the family-membership invariant: a deterministic consequence of the weight-zero
            // gates, so it holds for every note on every seed
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
    public void Compose_WithAChordalTexture_TheAccompanimentStaysFigureFreeWhileTheMelodyFigures()
    {
        foreach (var seed in Enumerable.Range(1, 4))
        {
            // arrange & act
            var composerGraph = ComposerGraph.Create(GetConfiguration(TextureType.Chordal), seed);
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var allNotes = composition.Measures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .ToList();

            var accompanimentNotes = allNotes
                .Where(note => composerGraph.Ledger.IsHeldNote(note) || composerGraph.Ledger.IsTextureFigurationNote(note))
                .ToList();

            accompanimentNotes.Should().NotBeEmpty($"a chordal texture must record pads and a figuration voice (seed {seed})");

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

            allNotes
                .Exists(note => composerGraph.Ledger.IsFloridNote(note) && note.Ornamentations.Count > 0)
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

            var recordedNotes = composition.Measures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .ToList();

            var melodyVelocities = recordedNotes.Where(composerGraph.Ledger.IsFloridNote).Select(static note => (int)note.Velocity).ToList();
            var accompanimentVelocities = recordedNotes
                .Where(note => composerGraph.Ledger.IsHeldNote(note) || composerGraph.Ledger.IsTextureFigurationNote(note))
                .Select(static note => (int)note.Velocity)
                .ToList();

            // assert - the prominence offset separates the means on every seed: the melody's strong-beat
            // accents push it up while the accompaniment sits a full offset lower, a wide margin against
            // the velocity walk's noise
            melodyVelocities.Should().NotBeEmpty($"the melody records florid under a texture (seed {seed})");
            accompanimentVelocities.Should().NotBeEmpty($"the accompaniment records under a texture (seed {seed})");
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
            texturedNotes.Should().NotBeEquivalentTo(untexturedNotes, $"an active texture must change the rendered composition (seed {seed})");
        }
    }

    // Production-shaped default ranges rather than the narrow test ranges: the broken-chord family's octave
    // figures need the octave headroom the redesigned defaults guarantee, and the listening artifacts
    // should mirror what the app renders. Constructed fresh - InstrumentConfigurations feeds property
    // initializers a with-clone would leave stale.
    private static CompositionConfiguration GetConfiguration(
        TextureType texture,
        bool voiceRhythmEnabled = true,
        GroundBassConfiguration? groundBassConfiguration = null) => new(
        new HashSet<InstrumentConfiguration>
        {
            InstrumentConfiguration.DefaultConfigurations[Instrument.One],
            InstrumentConfiguration.DefaultConfigurations[Instrument.Two],
            InstrumentConfiguration.DefaultConfigurations[Instrument.Three]
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
        GroundBassConfiguration: groundBassConfiguration,
        VoiceRhythmConfiguration: new VoiceRhythmConfiguration(voiceRhythmEnabled),
        TextureConfiguration: new TextureConfiguration(texture));

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
