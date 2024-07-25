using Atrea.PolicyEngine;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Collections;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Output;

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
        var sopranoC4WithPassingTone = new BaroquenNote(Voice.Soprano, Notes.C4)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.D4) }
        };

        var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);

        var tenorA2 = new BaroquenNote(Voice.Tenor, Notes.A2);

        var bassF1 = new BaroquenNote(Voice.Bass, Notes.F1);

        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
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
