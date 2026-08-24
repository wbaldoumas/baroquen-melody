using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors.Factories;

[TestFixture]
internal sealed class OrnamentationProcessorFactoryTests
{
    [Test]
    public void Create_RunsTheSharedLoggingPolicyBeforeTheCleaningPolicyOnEveryProcessor()
    {
        // arrange - a single always-firing passing tone, so exactly one processor applies and logs
        var compositionConfiguration = TestCompositionConfigurations.Get(2) with
        {
            AggregateOrnamentationConfiguration = new AggregateOrnamentationConfiguration(
                new HashSet<OrnamentationConfiguration> { new(OrnamentationType.PassingTone, ConfigurationStatus.Enabled, Probability: 100) }
            )
        };

        var logger = new CapturingLogger();
        var messagesLoggedWhenCleaningRan = -1;

        var cleaningPolicy = Substitute.For<IOutputPolicy<OrnamentationItem>>();
        cleaningPolicy.When(static policy => policy.Apply(Arg.Any<OrnamentationItem>())).Do(_ => messagesLoggedWhenCleaningRan = logger.Messages.Count);

        var voiceRhythmPolicyTransformer = Substitute.For<IVoiceRhythmPolicyTransformer>();
        voiceRhythmPolicyTransformer
            .Transform(Arg.Any<OrnamentationConfiguration>(), Arg.Any<IInputPolicy<OrnamentationItem>[]>())
            .Returns(static callInfo => callInfo.ArgAt<IInputPolicy<OrnamentationItem>[]>(1));

        var factory = new OrnamentationProcessorFactory(
            new MusicalTimeSpanCalculator(),
            new OrnamentationProcessorConfigurationFactory(new ChordNumberIdentifier(compositionConfiguration), new WeightedRandomBooleanGenerator(new SeededRandomProvider(1)), compositionConfiguration),
            new LogAppliedOrnamentation(logger),
            cleaningPolicy,
            voiceRhythmPolicyTransformer
        );

        var item = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]))
        );

        // act
        foreach (var processor in factory.Create(compositionConfiguration))
        {
            processor.Process(item);
        }

        // assert
        item.CurrentBeat[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        logger.Messages.Should().ContainSingle().Which.Should().Be("Ornamentation PassingTone applied to instrument One.");
        cleaningPolicy.Received(1).Apply(item);
        messagesLoggedWhenCleaningRan.Should().Be(1, "the applied ornamentation is logged before the cleaner may strip it again");
    }
}
