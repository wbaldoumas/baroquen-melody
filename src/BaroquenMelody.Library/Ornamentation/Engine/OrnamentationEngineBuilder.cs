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
using LazyCart;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder
{
    private readonly IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

    private readonly IInputPolicy<OrnamentationItem> _hasNoOrnamentation = new Not<OrnamentationItem>(new HasOrnamentation());

    private readonly NoteIndexPairSelector _noteIndexPairSelector;

    private readonly OrnamentationProcessorFactory _processorFactory;

    private readonly CompositionConfiguration _compositionConfiguration;

    private readonly IMusicalTimeSpanCalculator _musicalTimeSpanCalculator;

    private readonly ILogger _logger;

    public OrnamentationEngineBuilder(
        CompositionConfiguration compositionConfiguration,
        IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
        ILogger logger)
    {
        _compositionConfiguration = compositionConfiguration;
        _musicalTimeSpanCalculator = musicalTimeSpanCalculator;
        _logger = logger;
        _noteIndexPairSelector = new NoteIndexPairSelector(new NoteOnsetCalculator(musicalTimeSpanCalculator, compositionConfiguration));

        _processorFactory = new OrnamentationProcessorFactory(
            musicalTimeSpanCalculator,
            new OrnamentationProcessorConfigurationFactory(
                new ChordNumberIdentifier(compositionConfiguration),
                new WeightedRandomBooleanGenerator(),
                compositionConfiguration,
                logger
            ),
            new CleanConflictingOrnamentations(BuildOrnamentationCleaningEngine())
        );
    }

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(_processorFactory.Create(_compositionConfiguration).ToArray())
        .WithoutOutputPolicies()
        .Build();

    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(_weightedRandomBooleanGenerator),
            new IsRepeatedNote(),
            _hasNoOrnamentation,
            new IsApplicableInterval(_compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(_musicalTimeSpanCalculator, _compositionConfiguration))
        .WithOutputPolicies(new LogOrnamentation(OrnamentationType.Sustain, _logger))
        .Build();

    private IPolicyEngine<OrnamentationCleaningItem> BuildOrnamentationCleaningEngine()
    {
        var ornamentationTypes = EnumUtils<OrnamentationType>
            .AsEnumerable()
            .Where(ornamentationType => ornamentationType is not OrnamentationType.None
                and not OrnamentationType.Sustain
                and not OrnamentationType.MidSustain
                and not OrnamentationType.Rest
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
