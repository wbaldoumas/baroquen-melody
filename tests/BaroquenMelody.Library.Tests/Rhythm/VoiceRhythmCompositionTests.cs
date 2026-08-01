using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Builders;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rhythm;

/// <summary>
///     End-to-end anchors for per-voice rhythm roles. Seeded walks differ across operating systems, so every
///     feature-on assertion is an existence sweep or an all-notes invariant, never a per-seed outcome pin;
///     the byte-identity anchor compares two same-process renders, not a recorded baseline.
/// </summary>
[TestFixture]
internal sealed class VoiceRhythmCompositionTests
{
    private static readonly OrnamentationType[] SixteenthTierOrnamentations =
    [
        OrnamentationType.DoubleTurn,
        OrnamentationType.DoubleInvertedTurn,
        OrnamentationType.DoubleRun,
        OrnamentationType.SequencedThirds,
        OrnamentationType.DoublePedalPassingTone,
        OrnamentationType.Trill
    ];

    private static readonly OrnamentationType[] AllowedHeldOrnamentations =
    [
        OrnamentationType.None,
        OrnamentationType.Sustain,
        OrnamentationType.MidSustain,
        OrnamentationType.Suspension,
        OrnamentationType.SuspensionResolution
    ];

    [Test]
    public void Compose_WithVoiceRhythmToggled_ProducesDifferentCompositions()
    {
        foreach (var seed in Enumerable.Range(1, 5))
        {
            // arrange
            var enabledConfiguration = CreateConfiguration(voiceRhythmEnabled: true);
            var disabledConfiguration = CreateConfiguration(voiceRhythmEnabled: false);

            // act
            var enabledNotes = SeededComposition.Notes(SeededComposition.Compose(enabledConfiguration, seed));
            var disabledNotes = SeededComposition.Notes(SeededComposition.Compose(disabledConfiguration, seed));

            // assert
            enabledNotes.Should().NotBeEquivalentTo(disabledNotes, $"voice rhythm roles must change the rendered composition (seed {seed})");
        }
    }

    [Test]
    public void Compose_WithAllStandardRoles_IsByteIdenticalToDisabled()
    {
        foreach (var seed in Enumerable.Range(1, 3))
        {
            // arrange - the feature enabled with a scheduler that assigns no roles must compose exactly the
            // disabled composition: no pinned searches fire, the ledger stays empty, and the role-aware gates
            // draw once at the standard weights just as the standard gates do
            var allStandardScheduler = Substitute.For<IVoiceRhythmScheduler>();
            var enabledGraph = ComposerGraph.Create(CreateConfiguration(voiceRhythmEnabled: true), seed, allStandardScheduler);
            var disabledGraph = ComposerGraph.Create(CreateConfiguration(voiceRhythmEnabled: false), seed, voiceRhythmSchedulerOverride: null);

            // act
            var enabledComposition = enabledGraph.Composer.Compose(CancellationToken.None);
            var disabledComposition = disabledGraph.Composer.Compose(CancellationToken.None);

            // assert
            var enabledNotes = SnapshotNotes(enabledGraph.Configuration, enabledComposition);
            var disabledNotes = SnapshotNotes(disabledGraph.Configuration, disabledComposition);

            enabledNotes.Should().Equal(disabledNotes, $"all-standard roles must degenerate to the disabled composition note for note (seed {seed})");
        }
    }

    [Test]
    public void Compose_WithVoiceRhythm_KeepsHeldRunsCleanAndEventuallyTiesThem()
    {
        var tieRealized = false;

        foreach (var seed in Enumerable.Range(1, 8))
        {
            // arrange
            var composerGraph = ComposerGraph.Create(CreateConfiguration(voiceRhythmEnabled: true), seed, voiceRhythmSchedulerOverride: null);

            // act
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            var heldNotes = composition.Measures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .Where(composerGraph.Ledger.IsHeldNote)
                .ToList();

            // assert - the suppression invariant holds for EVERY held note on every seed: no ornament figure
            // may break a held run (the sustain stamps and the suspension pass's restamps are the run's own
            // mechanics, not breaks)
            heldNotes.Should().NotBeEmpty($"held roles must be scheduled and recorded (seed {seed})");

            foreach (var heldNote in heldNotes)
            {
                AllowedHeldOrnamentations.Should().Contain(heldNote.OrnamentationType, $"a held run must stay figure-free (seed {seed})");
                heldNote.Ornamentations.Should().BeEmpty($"a held note carries no sub-notes (seed {seed})");
            }

            tieRealized |= heldNotes.Exists(static heldNote => heldNote.OrnamentationType == OrnamentationType.Sustain);
        }

        // the existence half: across the sweep, at least one held pair actually ties into a sustained tone
        tieRealized.Should().BeTrue("some held run across the seed sweep must realize its tie");
    }

    [Test]
    public void Compose_WithVoiceRhythm_EventuallyLandsSixteenthTierFiguresOnFloridNotes()
    {
        var floridFigureRealized = false;

        foreach (var seed in Enumerable.Range(1, 8))
        {
            // arrange
            var composerGraph = ComposerGraph.Create(CreateConfiguration(voiceRhythmEnabled: true), seed, voiceRhythmSchedulerOverride: null);

            // act
            var composition = composerGraph.Composer.Compose(CancellationToken.None);

            floridFigureRealized |= composition.Measures
                .SelectMany(static measure => measure.Beats)
                .SelectMany(static beat => beat.Chord.Notes)
                .Any(note => composerGraph.Ledger.IsFloridNote(note) && SixteenthTierOrnamentations.Contains(note.OrnamentationType));

            if (floridFigureRealized)
            {
                break;
            }
        }

        // assert
        floridFigureRealized.Should().BeTrue("the boosted sixteenth tier must land on a florid voice's note somewhere in the seed sweep");
    }

    [Test]
    public void Compose_WithAGroundPatternThatCannotFit_FallsBackToTheFugueWithRolesActive()
    {
        // arrange - a seven-semitone bass range cannot host the octave-spanning romanesca, so the planner
        // yields no plan and the ground form falls back to the fugal composer, which runs with roles active
        var instrumentConfigurations = new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C4, Notes.C6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Two, Notes.G2, Notes.G4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Three, Notes.C2, Notes.G2, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        };

        var compositionConfiguration = new CompositionConfiguration(
            instrumentConfigurations,
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 25,
            ShuffleOrnamentationProcessors: false,
            GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca));

        // act
        var midiFileComposition = SeededComposition.Compose(compositionConfiguration, seed: 1);

        // assert
        SeededComposition.Notes(midiFileComposition).Should().NotBeEmpty("the fallback fugue must compose successfully with roles active");
    }

    private static CompositionConfiguration CreateConfiguration(bool voiceRhythmEnabled) => TestCompositionConfigurations.Get(numberOfInstruments: 3, compositionLength: 25) with
    {
        ShuffleOrnamentationProcessors = false,
        VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled)
    };

    private static List<MidiNoteSnapshot> SnapshotNotes(CompositionConfiguration compositionConfiguration, Composition composition) =>
        new MidiGenerator(compositionConfiguration).Generate(composition)
            .GetNotes()
            .Select(static note => new MidiNoteSnapshot((int)note.NoteNumber, (int)note.Velocity, note.Time, note.Length))
            .ToList();

    /// <summary>
    ///     The full fugal component graph, hand-wired exactly as the configurator wires it, exposing the
    ///     composition object and the ledger that <see cref="BaroquenMelodyComposerConfigurator"/> keeps
    ///     internal - plus a scheduler override seam for the all-standard degeneration anchor.
    /// </summary>
    private sealed record ComposerGraph(Composer Composer, VoiceRhythmLedger Ledger, CompositionConfiguration Configuration)
    {
        public static ComposerGraph Create(CompositionConfiguration configuration, int seed, IVoiceRhythmScheduler? voiceRhythmSchedulerOverride)
        {
            var randomProvider = new SeededRandomProvider(seed);
            var weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);
            var logger = Substitute.For<ILogger<MidiFileComposition>>();
            var dispatcher = Substitute.For<IDispatcher>();
            var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();
            var voiceSpacingSatisfiabilityAnalyzer = new VoiceSpacingSatisfiabilityAnalyzer();

            var chordNumberIdentifier = new ChordNumberIdentifier(configuration);
            var chordInversionIdentifier = new ChordInversionIdentifier(chordNumberIdentifier, configuration);
            var compositionRuleFactory = new CompositionRuleFactory(configuration, weightedRandomBooleanGenerator, chordNumberIdentifier);
            var compositionRule = compositionRuleFactory.CreateAggregate(configuration.AggregateCompositionRuleConfiguration);
            var compositionStrategy = new CompositionStrategyFactory(new NoteChoiceGenerator(), compositionRule, randomProvider, logger).Create(configuration);

            var fugalEntryRuleConfiguration = new AggregateCompositionRuleConfiguration(
                configuration.AggregateCompositionRuleConfiguration.Configurations
                    .Where(static ruleConfiguration => ruleConfiguration.Rule != CompositionRule.EnforceVoiceSpacing)
                    .ToHashSet());

            var fugalEntryStrategy = new CompositionStrategyFactory(new NoteChoiceGenerator(), compositionRuleFactory.CreateAggregate(fugalEntryRuleConfiguration), randomProvider, logger).Create(configuration);

            var voiceRhythmLedger = new VoiceRhythmLedger();
            var voiceRhythmScheduler = voiceRhythmSchedulerOverride ?? new VoiceRhythmScheduler(configuration);
            var ornamentationEngineBuilder = new OrnamentationEngineBuilder(configuration, musicalTimeSpanCalculator, randomProvider, logger, voiceRhythmLedger);
            var compositionDecorator = new CompositionDecorator(ornamentationEngineBuilder.BuildOrnamentationEngine(), ornamentationEngineBuilder.BuildSustainedNoteEngine(), configuration);
            var dynamicsApplicator = new DynamicsApplicator(configuration, new DynamicsEngineBuilder(configuration, randomProvider).Build());
            var motifBankFactory = new MotifBankFactory(new MotifExtractor(configuration), configuration);
            var motifDeveloper = new MotifDeveloper(new MotifApplicator(configuration), weightedRandomBooleanGenerator, randomProvider, configuration);
            var cadenceClassifier = new CadenceClassifier(chordNumberIdentifier, chordInversionIdentifier, configuration);

            var cadentialTrillApplicator = new CadentialTrillApplicator(
                cadenceClassifier,
                new OrnamentationProcessorConfigurationFactory(chordNumberIdentifier, weightedRandomBooleanGenerator, configuration, logger),
                musicalTimeSpanCalculator,
                configuration);

            var compositionPhraser = new CompositionPhraser(compositionRule, new ThemeSplitter(), weightedRandomBooleanGenerator, randomProvider, logger, configuration, motifBankFactory, motifDeveloper, cadentialTrillApplicator);
            var fugalEntryPlacer = new FugalEntryPlacer(configuration, voiceSpacingSatisfiabilityAnalyzer.GetFeasibleNoteNumbersByInstrument(configuration));
            var scoringRuleFactory = new ScoringRuleFactory(configuration, chordNumberIdentifier, chordInversionIdentifier);
            var chordSelector = new WeightedChordSelector(scoringRuleFactory.CreateAggregate(AggregateScoringRuleConfiguration.Default), randomProvider);
            var chordComposer = new ChordComposer(compositionStrategy, chordSelector, logger);
            var themeComposer = new ThemeComposer(compositionStrategy, fugalEntryStrategy, compositionDecorator, chordComposer, fugalEntryPlacer, new FugalAnswerStrategy(configuration), chordSelector, dispatcher, logger, configuration);
            var endingComposer = new EndingComposer(compositionStrategy, compositionDecorator, chordNumberIdentifier, cadenceClassifier, cadentialTrillApplicator, chordSelector, dispatcher, logger, configuration);

            var composer = new Composer(
                compositionDecorator,
                compositionPhraser,
                chordComposer,
                new HarmonicRhythmScheduler(configuration),
                voiceRhythmScheduler,
                voiceRhythmLedger,
                new SuspensionApplicator(weightedRandomBooleanGenerator, configuration),
                new TonicizationApplicator(chordNumberIdentifier, weightedRandomBooleanGenerator, configuration),
                themeComposer,
                endingComposer,
                dynamicsApplicator,
                dispatcher,
                configuration);

            return new ComposerGraph(composer, voiceRhythmLedger, configuration);
        }
    }
}
