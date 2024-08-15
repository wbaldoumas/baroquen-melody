using Atrea.PolicyEngine;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Tests.TestData;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation;

[TestFixture]
internal sealed class CompositionDecoratorTests
{
    private IPolicyEngine<OrnamentationItem> _mockOrnamentationEngine = null!;

    private IPolicyEngine<OrnamentationItem> _mockSustainEngine = null!;

    private CompositionDecorator _compositionDecorator = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _mockOrnamentationEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();
        _mockSustainEngine = Substitute.For<IPolicyEngine<OrnamentationItem>>();

        _compositionDecorator = new CompositionDecorator(_mockOrnamentationEngine, _mockSustainEngine, compositionConfiguration);
    }

    [Test]
    public void WhenDecorateIsInvoked_ThenOrnamentationEngineIsInvoked_ForEachVoiceAndChord()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
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
    public void WhenDecorateIsInvokedForSpecificVoice_ThenOrnamentationEngineIsInvokedForVoice()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
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
        _compositionDecorator.Decorate(composition, Voice.One);

        // assert
        _mockOrnamentationEngine.ReceivedWithAnyArgs(4).Process(Arg.Any<OrnamentationItem>());
        _mockSustainEngine.DidNotReceiveWithAnyArgs().Process(Arg.Any<OrnamentationItem>());
    }

    [Test]
    public void WhenApplySustainIsInvoked_ThenSustainEngineIsInvoked_ForEachVoiceAndChord()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Two, Notes.C3, MusicalTimeSpan.Half)
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
