using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Processors;
using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Configuration;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Rhythm;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder
{
    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator;

    private readonly IInputPolicy<OrnamentationItem> _hasNoOrnamentation = new Not<OrnamentationItem>(new HasOrnamentation());

    private readonly NoteIndexPairSelector _noteIndexPairSelector;

    private readonly OrnamentationProcessorFactory _processorFactory;

    private readonly CompositionConfiguration _compositionConfiguration;

    private readonly IMusicalTimeSpanCalculator _musicalTimeSpanCalculator;

    private readonly ILogger _logger;

    private readonly VoiceRhythmPolicyTransformer _voiceRhythmPolicyTransformer;

    public OrnamentationEngineBuilder(
        CompositionConfiguration compositionConfiguration,
        IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
        IRandomProvider randomProvider,
        ILogger logger,
        IVoiceRhythmLedger voiceRhythmLedger)
    {
        _compositionConfiguration = compositionConfiguration;
        _musicalTimeSpanCalculator = musicalTimeSpanCalculator;
        _logger = logger;
        _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);
        _noteIndexPairSelector = new NoteIndexPairSelector(new NoteOnsetCalculator(musicalTimeSpanCalculator, compositionConfiguration));
        _voiceRhythmPolicyTransformer = new VoiceRhythmPolicyTransformer(_weightedRandomBooleanGenerator, voiceRhythmLedger, compositionConfiguration);

        _processorFactory = new OrnamentationProcessorFactory(
            musicalTimeSpanCalculator,
            new OrnamentationProcessorConfigurationFactory(
                new ChordNumberIdentifier(compositionConfiguration),
                _weightedRandomBooleanGenerator,
                compositionConfiguration,
                logger
            ),
            new CleanConflictingOrnamentations(BuildOrnamentationCleaner()),
            _voiceRhythmPolicyTransformer
        );
    }

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(_processorFactory.Create(_compositionConfiguration).ToArray())
        .WithoutOutputPolicies()
        .Build();

    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            _voiceRhythmPolicyTransformer.CreateSustainGate(),
            new IsRepeatedNote(),
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(_compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Sustain, _logger))
        .Build();

    /// <summary>
    ///     Builds the cross-voice cleaner: one <see cref="OrnamentationCleaner"/> per ordered pair of cleanable
    ///     ornamentation types, keyed for direct dispatch on the item's own pair.
    /// </summary>
    /// <returns>The keyed ornamentation cleaner.</returns>
    public IProcessor<OrnamentationCleaningItem> BuildOrnamentationCleaner()
    {
        var ornamentationTypes = EnumUtils<OrnamentationType>
            .AsEnumerable()
            .Where(ornamentationType => ornamentationType is not OrnamentationType.None
                and not OrnamentationType.Sustain
                and not OrnamentationType.MidSustain
                and not OrnamentationType.Rest

                // Suspension stamps are applied only after every decoration pass has run, so the cleaning
                // engine can never encounter them.
                and not OrnamentationType.Suspension
                and not OrnamentationType.SuspensionResolution
            )
            .ToList();

        var cleaningSelector = new NoteTargetSelector(
            [
                new CleanTargetOrnamentation(),
                new CleanLowerNote(),
                new CleanRandomNote(_weightedRandomBooleanGenerator)
            ]
        );

        var cleanersByOrnamentationPair = new Dictionary<(OrnamentationType Note, OrnamentationType OtherNote), IProcessor<OrnamentationCleaningItem>>();

        foreach (var primaryOrnamentation in ornamentationTypes)
        {
            foreach (var secondaryOrnamentation in ornamentationTypes)
            {
                var noteSelector = new NotePairSelector(primaryOrnamentation, secondaryOrnamentation);
                var indices = _noteIndexPairSelector.Select(primaryOrnamentation, secondaryOrnamentation);

                var ornamentationCleaningConfiguration = new OrnamentationCleanerConfiguration(
                    noteSelector,
                    indices,
                    cleaningSelector
                );

                cleanersByOrnamentationPair[(primaryOrnamentation, secondaryOrnamentation)] = new OrnamentationCleaner(ornamentationCleaningConfiguration, _compositionConfiguration, _weightedRandomBooleanGenerator);
            }
        }

        return new OrnamentationCleaningDispatcher(cleanersByOrnamentationPair.ToFrozenDictionary());
    }
}
