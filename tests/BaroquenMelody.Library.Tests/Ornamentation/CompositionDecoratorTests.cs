using Atrea.PolicyEngine;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Tests.TestData;
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

    private CompositionDecorator _compositionDecorator = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(2);

        _mockOrnamentationEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        _mockSustainEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();

        _compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, compositionConfiguration);
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
}
