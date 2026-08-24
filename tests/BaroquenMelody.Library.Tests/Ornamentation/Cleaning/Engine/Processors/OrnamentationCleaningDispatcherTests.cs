using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Processors;

[TestFixture]
internal sealed class OrnamentationCleaningDispatcherTests
{
    private IProcessor<OrnamentationCleaningItem> _passingTonePassingToneCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _passingToneTurnCleaner = null!;

    private OrnamentationCleaningDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _passingTonePassingToneCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _passingToneTurnCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();

        _dispatcher = new OrnamentationCleaningDispatcher(
            new Dictionary<(OrnamentationType, OrnamentationType), IProcessor<OrnamentationCleaningItem>>
            {
                [(OrnamentationType.PassingTone, OrnamentationType.PassingTone)] = _passingTonePassingToneCleaner,
                [(OrnamentationType.PassingTone, OrnamentationType.Turn)] = _passingToneTurnCleaner
            }.ToFrozenDictionary()
        );
    }

    [Test]
    public void Process_RoutesTheItemToTheCleanerRegisteredForItsOrnamentationPair()
    {
        // arrange
        var item = CreateItem(OrnamentationType.PassingTone, OrnamentationType.Turn);

        // act
        _dispatcher.Process(item);

        // assert
        _passingToneTurnCleaner.Received(1).Process(item);
        _passingTonePassingToneCleaner.DidNotReceiveWithAnyArgs().Process(default!);
    }

    [Test]
    public void Process_KeysThePairInNoteThenOtherNoteOrder()
    {
        // arrange - (Turn, PassingTone) is a different pair from (PassingTone, Turn) and has no cleaner registered
        var item = CreateItem(OrnamentationType.Turn, OrnamentationType.PassingTone);

        // act
        _dispatcher.Process(item);

        // assert
        _passingToneTurnCleaner.DidNotReceiveWithAnyArgs().Process(default!);
        _passingTonePassingToneCleaner.DidNotReceiveWithAnyArgs().Process(default!);
    }

    [Test]
    public void Process_DoesNothingForAnUnregisteredPair()
    {
        // arrange - un-ornamented notes never have a cleaner
        var item = CreateItem(OrnamentationType.None, OrnamentationType.PassingTone);

        // act
        var act = () => _dispatcher.Process(item);

        // assert
        Assert.DoesNotThrow(() => act());
        _passingToneTurnCleaner.DidNotReceiveWithAnyArgs().Process(default!);
        _passingTonePassingToneCleaner.DidNotReceiveWithAnyArgs().Process(default!);
    }

    private static OrnamentationCleaningItem CreateItem(OrnamentationType noteOrnamentationType, OrnamentationType otherNoteOrnamentationType) => new(
        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { OrnamentationType = noteOrnamentationType },
        new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half) { OrnamentationType = otherNoteOrnamentationType }
    );
}
