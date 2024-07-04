using Atrea.PolicyEngine;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
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
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.A4, Notes.A5),
                new(Voice.Alto, Notes.C3, Notes.C4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

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
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
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
    public void WhenApplySustainIsInvoked_ThenSustainEngineIsInvoked_ForEachVoiceAndChord()
    {
        // arrange
        var chordA = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordB = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordC = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
            ]
        );

        var chordD = new BaroquenChord(
            [
                new BaroquenNote(Voice.Soprano, Notes.A4),
                new BaroquenNote(Voice.Alto, Notes.C3)
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
