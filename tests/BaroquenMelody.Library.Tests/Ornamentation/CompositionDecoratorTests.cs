using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation;

[TestFixture]
internal sealed class CompositionDecoratorTests
{
    private IPolicyEngine<OrnamentationItem> _mockOrnamentationEngine = null!;

    private IPolicyEngine<OrnamentationItem> _mockSustainEngine = null!;

    private IVoiceRhythmScheduler _mockVoiceRhythmScheduler = null!;

    private CompositionDecorator _compositionDecorator = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _mockOrnamentationEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        _mockSustainEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        _mockVoiceRhythmScheduler = Substitute.For<IVoiceRhythmScheduler>();

        _compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration, new SeededRandomProvider(1));
    }

    [Test]
    public void WhenDecorateIsInvoked_ThenOrnamentationEngineIsInvoked_ForEachInstrumentAndChord()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(chordA),
                        new Beat(chordB),
                        new Beat(chordC),
                        new Beat(chordD)
                    ],
                    Meter.FourFour
                )
            ]
        );

        // act
        _compositionDecorator.Decorate(composition);

        // assert
        _mockOrnamentationEngine.ReceivedWithAnyArgs(8).Process(Arg.Any<OrnamentationItem>());
        _mockSustainEngine.DidNotReceiveWithAnyArgs().Process(Arg.Any<OrnamentationItem>());
    }

    [Test]
    public void WhenDecorateIsInvokedForSpecificInstrument_ThenOrnamentationEngineIsInvokedForInstrument()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(chordA),
                        new Beat(chordB),
                        new Beat(chordC),
                        new Beat(chordD)
                    ],
                    Meter.FourFour
                )
            ]
        );

        // act
        _compositionDecorator.Decorate(composition, Instrument.One);

        // assert
        _mockOrnamentationEngine.ReceivedWithAnyArgs(4).Process(Arg.Any<OrnamentationItem>());
        _mockSustainEngine.DidNotReceiveWithAnyArgs().Process(Arg.Any<OrnamentationItem>());
    }

    [Test]
    public void WhenApplySustainIsInvoked_ThenSustainEngineIsInvoked_ForEachInstrumentAndChord()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(chordA),
                        new Beat(chordB),
                        new Beat(chordC),
                        new Beat(chordD)
                    ],
                    Meter.FourFour
                )
            ]
        );

        // act
        _compositionDecorator.ApplySustain(composition);

        // assert
        _mockSustainEngine.ReceivedWithAnyArgs(8).Process(Arg.Any<OrnamentationItem>());
        _mockOrnamentationEngine.DidNotReceiveWithAnyArgs().Process(Arg.Any<OrnamentationItem>());
    }

    [Test]
    public void WhenDecorateIsInvoked_AndShuffleOrnamentationProcessorsIsTrue_ThenTheProcessorsAreReplacedWithAPermutationAtEveryBeat()
    {
        // arrange
        var processors = CreateProcessors(5);
        var replacements = CaptureReplacements(_mockOrnamentationEngine, processors);
        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.Decorate(composition);

        // assert
        _mockOrnamentationEngine.DidNotReceive().Shuffle();
        replacements.Should().HaveCount(8, "every (beat, instrument) item re-orders the processors once");

        foreach (var replacement in replacements)
        {
            replacement.Should().HaveCount(processors.Count);
            replacement.Should().OnlyHaveUniqueItems();
            replacement.Should().OnlyContain(processor => processors.Contains(processor), "a shuffle is a permutation of the configured processors");
        }

        replacements.Should().Contain(replacement => !replacement.SequenceEqual(processors), "the seeded order must actually move the processors");
    }

    [Test]
    public void WhenDecorateIsInvoked_WithTheSameShuffleSeed_ThenTheProcessorOrdersAreReproducible()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2);
        var processors = CreateProcessors(5);

        var firstEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        var secondEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        var firstReplacements = CaptureReplacements(firstEngine, processors);
        var secondReplacements = CaptureReplacements(secondEngine, processors);

        var firstDecorator = new CompositionDecorator(firstEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration, new SeededRandomProvider(42));
        var secondDecorator = new CompositionDecorator(secondEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration, new SeededRandomProvider(42));

        // act
        firstDecorator.Decorate(CreateTwoInstrumentComposition());
        secondDecorator.Decorate(CreateTwoInstrumentComposition());

        // assert
        firstReplacements.Should().HaveCount(8);
        secondReplacements.Should().HaveCount(8);
        firstReplacements.Zip(secondReplacements).Should().AllSatisfy(pair => pair.First.Should().Equal(pair.Second));
    }

    [Test]
    public void WhenDecorateIsInvokedForASpecificInstrument_AndShuffleIsOn_ThenTheProcessorsAreReplacedWithAPermutation()
    {
        // arrange
        var processors = CreateProcessors(5);
        var replacements = CaptureReplacements(_mockOrnamentationEngine, processors);
        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.Decorate(composition, Instrument.One);

        // assert
        _mockOrnamentationEngine.DidNotReceive().Shuffle();
        replacements.Should().HaveCount(4, "the per-instrument pass re-orders the processors once per beat");

        foreach (var replacement in replacements)
        {
            replacement.Should().HaveCount(processors.Count);
            replacement.Should().OnlyHaveUniqueItems();
            replacement.Should().OnlyContain(processor => processors.Contains(processor));
        }
    }

    [Test]
    public void WhenDecorateIsInvoked_AndShuffleOrnamentationProcessorsIsFalse_ThenTheOrnamentationEngineIsNeverReordered()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2) with { ShuffleOrnamentationProcessors = false };
        var compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration, new SeededRandomProvider(1));
        var composition = CreateTwoInstrumentComposition();

        // act
        compositionDecorator.Decorate(composition);

        // assert
        _mockOrnamentationEngine.DidNotReceive().Shuffle();
        _mockOrnamentationEngine.DidNotReceiveWithAnyArgs().Replace(default!);
        _mockOrnamentationEngine.ReceivedWithAnyArgs(8).Process(Arg.Any<OrnamentationItem>());
    }

    [Test]
    public void WhenApplySustainIsInvoked_ThenTheSustainEngineIsNeverReordered()
    {
        // arrange
        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.ApplySustain(composition);

        // assert
        _mockSustainEngine.DidNotReceive().Shuffle();
        _mockSustainEngine.DidNotReceiveWithAnyArgs().Replace(default!);
    }

    [Test]
    public void WhenDecorateIsInvoked_AndATextureOrderExists_TheInstrumentsDecorateInThatOrder()
    {
        // arrange - the scheduler's texture order (melody first, figuration last) must drive the
        // whole-composition ornamentation pass, so cleaning conflicts always resolve in the melody's favor
        _mockVoiceRhythmScheduler
            .TryGetTextureDecorationOrder(out Arg.Any<IReadOnlyList<Instrument>>())
            .Returns(static callInfo =>
            {
                callInfo[0] = (IReadOnlyList<Instrument>)[Instrument.Two, Instrument.One];

                return true;
            });

        var processedInstruments = new List<Instrument>();

        _mockOrnamentationEngine
            .When(static engine => engine.Process(Arg.Any<OrnamentationItem>()))
            .Do(callInfo => processedInstruments.Add(callInfo.Arg<OrnamentationItem>().Instrument));

        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.Decorate(composition);

        // assert - four beats per instrument, the scheduler's order, not the configuration set's
        var expectedSequence = Enumerable.Repeat(Instrument.Two, 4).Concat(Enumerable.Repeat(Instrument.One, 4));

        processedInstruments.Should().Equal(expectedSequence);
    }

    [Test]
    public void WhenApplySustainIsInvoked_TheTextureOrderIsNeverConsulted()
    {
        // arrange - the sustain gate draws once per item, so re-ordering the sustain pass would re-order
        // its draws on the shared stream; only the ornamentation pass takes the texture order
        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.ApplySustain(composition);

        // assert
        _mockVoiceRhythmScheduler.DidNotReceive().TryGetTextureDecorationOrder(out Arg.Any<IReadOnlyList<Instrument>>());
        _mockSustainEngine.ReceivedWithAnyArgs(8).Process(Arg.Any<OrnamentationItem>());
    }

    private static Composition CreateTwoInstrumentComposition() => new(
        [
            new Measure(
                [
                    new Beat(CreateChord()),
                    new Beat(CreateChord()),
                    new Beat(CreateChord()),
                    new Beat(CreateChord())
                ],
                Meter.FourFour
            )
        ]
    );

    private static List<IProcessor<OrnamentationItem>> CreateProcessors(int count) =>
        Enumerable.Range(0, count).Select(static _ => Substitute.For<IProcessor<OrnamentationItem>>()).ToList();

    private static List<IReadOnlyCollection<IProcessor<OrnamentationItem>>> CaptureReplacements(IPolicyEngine<OrnamentationItem> engine, List<IProcessor<OrnamentationItem>> processors)
    {
        var replacements = new List<IReadOnlyCollection<IProcessor<OrnamentationItem>>>();

        engine.Processors.Returns(processors);

        engine.When(static policyEngine => policyEngine.Replace(Arg.Any<IReadOnlyCollection<IProcessor<OrnamentationItem>>>()))
            .Do(callInfo => replacements.Add(callInfo.Arg<IReadOnlyCollection<IProcessor<OrnamentationItem>>>()));

        return replacements;
    }

    private static BaroquenChord CreateChord() => new(
        [
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
        ]
    );
}
