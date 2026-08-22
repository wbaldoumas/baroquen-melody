using Atrea.PolicyEngine;
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

        _compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration);
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
    public void WhenDecorateIsInvoked_AndShuffleOrnamentationProcessorsIsTrue_ThenTheOrnamentationEngineIsShuffled()
    {
        // arrange
        var composition = CreateTwoInstrumentComposition();

        // act
        _compositionDecorator.Decorate(composition);

        // assert
        _mockOrnamentationEngine.Received(8).Shuffle();
    }

    [Test]
    public void WhenDecorateIsInvoked_AndShuffleOrnamentationProcessorsIsFalse_ThenTheOrnamentationEngineIsNotShuffled()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2) with { ShuffleOrnamentationProcessors = false };
        var compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, _mockVoiceRhythmScheduler, compositionConfiguration);
        var composition = CreateTwoInstrumentComposition();

        // act
        compositionDecorator.Decorate(composition);

        // assert
        _mockOrnamentationEngine.DidNotReceive().Shuffle();
        _mockOrnamentationEngine.ReceivedWithAnyArgs(8).Process(Arg.Any<OrnamentationItem>());
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

    private static BaroquenChord CreateChord() => new(
        [
            new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
        ]
    );
}
