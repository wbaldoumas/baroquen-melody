using Atrea.PolicyEngine.Policies.Output;
using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using System.Globalization;
using System.Text;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     Audit: determinism-1. A seeded composition must depend on its seed and configuration only, so every
///     draw-order-bearing enumeration must be a pure function of the configuration. These sets were once
///     <c>FrozenSet</c>s of records, and a frozen set enumerates in hash-bucket order; a record's hash folds in the
///     identity hash of its <c>EqualityContract</c> type, which the runtime assigns on first request from process
///     history — the walk therefore changed with whatever ran earlier in the process. The order pins here guard the
///     candidate, rule, and ornamentation-processor orders against any regression to history-dependent enumeration,
///     and the same-seed control guards against in-process nondeterminism creeping into the pipeline itself.
/// </summary>
[TestFixture]
internal sealed class SeededDeterminismTests
{
    [Test]
    public void NoteChoiceGenerator_EnumeratesCandidatesInGenerationOrder()
    {
        var expected = Enumerable
            .Range(1, CompositionConfiguration.MaxScaleStepChange)
            .SelectMany(static scaleStepChange => new[] { NoteMotion.Ascending, NoteMotion.Descending }.Select(noteMotion => new NoteChoice(Instrument.One, noteMotion, (byte)scaleStepChange)))
            .Append(new NoteChoice(Instrument.One, NoteMotion.Oblique, 0))
            .ToList();

        var actual = new NoteChoiceGenerator().GenerateNoteChoices(Instrument.One).ToList();

        actual.Should().Equal(expected, "the candidate order feeds every seeded draw, so it must not depend on hash-bucket order");
    }

    [Test]
    public void DefaultCompositionRules_EnumerateInDeclarationOrder()
    {
        var expected = EnumUtils<CompositionRule>.AsEnumerable().ToList();

        var actual = AggregateCompositionRuleConfiguration.Default.Configurations.Select(static configuration => configuration.Rule).ToList();

        actual.Should().Equal(expected, "rule order decides evaluation and draw order, so it must not depend on hash-bucket order");
    }

    [Test]
    public void CreateAggregate_GivenAdversariallyOrderedConfigurations_EvaluatesRulesInEnumOrder()
    {
        // arrange - each rule gets a distinct sub-threshold strictness, so its bypass wrapper identifies it by
        // drawing IsTrue(100 - strictness), and the configurations are handed over in reversed enum order: the
        // aggregate must evaluate in enum order no matter how the caller's set enumerates.
        var drawnWeights = new List<int>();
        var mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        mockWeightedRandomBooleanGenerator.IsTrue(Arg.Do<int>(drawnWeights.Add)).Returns(true);

        var factory = new CompositionRuleFactory(
            TestCompositionConfigurations.Get(2),
            mockWeightedRandomBooleanGenerator,
            Substitute.For<IChordNumberIdentifier>());

        var reversedConfigurations = EnumUtils<CompositionRule>
            .AsEnumerable()
            .Reverse()
            .Select(static rule => new CompositionRuleConfiguration(rule, ConfigurationStatus.Enabled, (int)rule))
            .ToHashSet();

        var aggregate = factory.CreateAggregate(new AggregateCompositionRuleConfiguration(reversedConfigurations));

        // act - the chord is in range for the two-instrument test configuration, so the prepended range guard
        // passes without drawing, and every bypass short-circuits true, drawing exactly once per rule in order.
        var chord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        aggregate.Evaluate([chord], chord);

        // assert
        var evaluatedRules = drawnWeights.Select(static weight => (CompositionRule)(100 - weight)).ToList();

        evaluatedRules.Should().Equal(
            EnumUtils<CompositionRule>.AsEnumerable().ToList(),
            "rule evaluation order fixes the seeded draw sequence, so it must be a pure function of the configuration");
    }

    [Test]
    public void Create_GivenAdversariallyOrderedConfigurations_CreatesProcessorsInEnumOrder()
    {
        // arrange - the ornamentation set enumerates in reversed enum order; the processor sequence the factory
        // yields must come out in enum order no matter how the configuration's set enumerates.
        var reversedConfigurations = AggregateOrnamentationConfiguration.Default.Configurations
            .OrderByDescending(static configuration => configuration.OrnamentationType)
            .ToHashSet();

        var configuration = TestCompositionConfigurations.Get(2) with
        {
            AggregateOrnamentationConfiguration = new AggregateOrnamentationConfiguration(reversedConfigurations),
        };

        var createdOrnamentationTypes = new List<OrnamentationType>();
        var mockConfigurationFactory = Substitute.For<IOrnamentationProcessorConfigurationFactory>();

        mockConfigurationFactory
            .Create(Arg.Do<OrnamentationConfiguration>(ornamentationConfiguration => createdOrnamentationTypes.Add(ornamentationConfiguration.OrnamentationType)))
            .Returns(Enumerable.Empty<OrnamentationProcessorConfiguration>());

        var factory = new OrnamentationProcessorFactory(
            Substitute.For<IMusicalTimeSpanCalculator>(),
            mockConfigurationFactory,
            Substitute.For<IOutputPolicy<OrnamentationItem>>(),
            Substitute.For<IVoiceRhythmPolicyTransformer>());

        // act
        _ = factory.Create(configuration).ToList();

        // assert
        var expected = reversedConfigurations
            .Where(static ornamentationConfiguration => ornamentationConfiguration.IsEnabled)
            .Select(static ornamentationConfiguration => ornamentationConfiguration.OrnamentationType)
            .OrderBy(static ornamentationType => ornamentationType)
            .ToList();

        createdOrnamentationTypes.Should().Equal(
            expected,
            "processor order is the engine's unshuffled traversal order, so it must be a pure function of the configuration");
    }

    [Test]
    public void Compose_SameSeedTwiceInOneProcess_ProducesTheSameNotes()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8, NoteName.A, Mode.Aeolian) with
        {
            ShuffleOrnamentationProcessors = false,
        };

        foreach (var seed in Enumerable.Range(1, 5))
        {
            var first = Fingerprint(ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None));
            var second = Fingerprint(ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None));

            second.Should().Be(first, $"seed {seed} composed twice in one process should be identical");
        }
    }

    /// <summary>
    ///     Serializes every principal and ornament note so two compositions can be compared exactly. The
    ///     cross-process experiment behind determinism-1 wrote this fingerprint to a file from one process that
    ///     composed seed 3 directly and from another that first ran a mocked <c>ComposerTests</c> case (or simply
    ///     requested a thousand identity hash codes); before the insertion-ordering fix the two pieces differed
    ///     from the second note of the subject.
    /// </summary>
    private static string Fingerprint(Composition composition)
    {
        var builder = new StringBuilder();

        foreach (var measure in composition.Measures)
        {
            foreach (var beat in measure.Beats)
            {
                foreach (var note in beat.Chord.Notes)
                {
                    AppendNote(builder, note);
                }

                builder.Append('|');
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static void AppendNote(StringBuilder builder, BaroquenNote note)
    {
        builder.Append(CultureInfo.InvariantCulture, $"{note.Instrument}:{note.Raw}:{note.MusicalTimeSpan}:{note.OrnamentationType}:{note.Velocity}");

        foreach (var ornamentation in note.Ornamentations)
        {
            builder.Append('(');
            AppendNote(builder, ornamentation);
            builder.Append(')');
        }

        builder.Append(';');
    }
}
