using Atrea.PolicyEngine;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Output;

[TestFixture]
internal sealed class CleanConflictingOrnamentationsTests
{
    private CleanConflictingOrnamentations _cleanConflictingOrnamentations = null!;

    private IPolicyEngine<OrnamentationCleaningItem> _mockOrnamentationCleaningEngine = null!;

    [SetUp]
    public void SetUp()
    {
        _mockOrnamentationCleaningEngine = Substitute.For<IPolicyEngine<OrnamentationCleaningItem>>();
        _cleanConflictingOrnamentations = new CleanConflictingOrnamentations(_mockOrnamentationCleaningEngine);
    }

    [Test]
    public void Apply_Invokes_Expected_Components_Expected_Number_Of_Times()
    {
        // arrange
        var sopranoC4WithPassingTone = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half) }
        };

        var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);

        var tenorA2 = new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half);

        var bassF1 = new BaroquenNote(Instrument.Four, Notes.F1, MusicalTimeSpan.Half);

        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([sopranoC4WithPassingTone, altoF3, tenorA2, bassF1])),
            null
        );

        // act
        _cleanConflictingOrnamentations.Apply(ornamentationItem);

        // assert
        _mockOrnamentationCleaningEngine.Received(6).Process(Arg.Any<OrnamentationCleaningItem>());
    }
}
