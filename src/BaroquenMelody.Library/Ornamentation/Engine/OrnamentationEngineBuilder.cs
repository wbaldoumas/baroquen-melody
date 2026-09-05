using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using Atrea.PolicyEngine.Processors;
using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Configuration;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Policies.Input;
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
using LazyCart;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    ///     Shared by every ornamentation engine and the sustain engine: it logs whatever type the processor just applied.
    /// </summary>
    private readonly IOutputPolicy<OrnamentationItem> _logAppliedOrnamentation;

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
        _logAppliedOrnamentation = new LogAppliedOrnamentation(logger);
        _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator(randomProvider);
        _noteIndexPairSelector = new NoteIndexPairSelector(new NoteOnsetCalculator(musicalTimeSpanCalculator, compositionConfiguration));
        _voiceRhythmPolicyTransformer = new VoiceRhythmPolicyTransformer(_weightedRandomBooleanGenerator, voiceRhythmLedger, compositionConfiguration);

        _processorFactory = new OrnamentationProcessorFactory(
            musicalTimeSpanCalculator,
            new OrnamentationProcessorConfigurationFactory(
                new ChordNumberIdentifier(compositionConfiguration),
                _weightedRandomBooleanGenerator,
                compositionConfiguration
            ),
            _logAppliedOrnamentation,
            new CleanConflictingOrnamentations(BuildOrnamentationCleaningEngine()),
            _voiceRhythmPolicyTransformer
        );
    }

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(_processorFactory.Create(_compositionConfiguration).ToArray())
        .WithoutOutputPolicies()
        .Build();

    // The probability gate must stay the first input policy: it draws once per item on the shared seeded
    // stream, so a deterministic policy placed ahead of it would remove draws and shift every later pass.
    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            _voiceRhythmPolicyTransformer.CreateSustainGate(),
            new IsRepeatedNote(),
            new Not<OrnamentationItem>(new HasCraftedTimeSpan(_compositionConfiguration)),
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(_compositionConfiguration))
        .WithOutputPolicies(_logAppliedOrnamentation)
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildOrnamentationCleaningEngine()
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

        var ornamentationCombinations = new LazyCartesianProduct<OrnamentationType, OrnamentationType>(ornamentationTypes, ornamentationTypes);

        var processors = new List<IProcessor<OrnamentationCleaningItem>>();

        for (var i = 0; i < ornamentationCombinations.Size; i++)
        {
            var (primaryOrnamentation, secondaryOrnamentation) = ornamentationCombinations[i];

            var noteSelector = new NotePairSelector(primaryOrnamentation, secondaryOrnamentation);
            var indices = _noteIndexPairSelector.Select(primaryOrnamentation, secondaryOrnamentation);

            var ornamentationCleaningConfiguration = new OrnamentationCleanerConfiguration(
                noteSelector,
                indices,
                cleaningSelector
            );

            var processor = PolicyEngineBuilder<OrnamentationCleaningItem>
                .Configure()
                .WithInputPolicies(new HasTargetOrnamentations(primaryOrnamentation, secondaryOrnamentation))
                .WithProcessors(new OrnamentationCleaner(ornamentationCleaningConfiguration, _compositionConfiguration, _weightedRandomBooleanGenerator))
                .Build();

            processors.Add(processor);
        }

        return PolicyEngineBuilder<OrnamentationCleaningItem>
            .Configure()
            .WithoutInputPolicies()
            .WithProcessors([.. processors])
            .WithOutputPolicies()
            .Build();
    }
}
