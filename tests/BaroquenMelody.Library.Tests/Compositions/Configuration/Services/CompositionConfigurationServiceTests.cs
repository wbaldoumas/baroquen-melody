using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

[TestFixture]
internal sealed class CompositionConfigurationServiceTests
{
    private IOrnamentationConfigurationService _mockOrnamentationConfigurationService = null!;

    private ICompositionRuleConfigurationService _mockCompositionRuleConfigurationService = null!;

    private IInstrumentConfigurationService _mockInstrumentConfigurationService = null!;

    private IDispatcher _mockDispatcher = null!;

    private CompositionConfigurationService _compositionConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockOrnamentationConfigurationService = Substitute.For<IOrnamentationConfigurationService>();
        _mockCompositionRuleConfigurationService = Substitute.For<ICompositionRuleConfigurationService>();
        _mockInstrumentConfigurationService = Substitute.For<IInstrumentConfigurationService>();
        _mockDispatcher = Substitute.For<IDispatcher>();

        _compositionConfigurationService = new CompositionConfigurationService(
            _mockOrnamentationConfigurationService,
            _mockCompositionRuleConfigurationService,
            _mockInstrumentConfigurationService,
            _mockDispatcher
        );
    }

    [Test]
    public void ConfigurableRootNotes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableRootNotes = new[]
        {
            NoteName.C,
            NoteName.CSharp,
            NoteName.D,
            NoteName.DSharp,
            NoteName.E,
            NoteName.F,
            NoteName.FSharp,
            NoteName.G,
            NoteName.GSharp,
            NoteName.A,
            NoteName.ASharp,
            NoteName.B
        };

        // act
        var actualConfigurableRootNotes = _compositionConfigurationService.ConfigurableRootNotes;

        // assert
        actualConfigurableRootNotes.Should().BeEquivalentTo(expectedConfigurableRootNotes);
    }

    [Test]
    public void ConfigurableScaleModes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableScaleModes = new[]
        {
            Mode.Ionian,
            Mode.Dorian,
            Mode.Phrygian,
            Mode.Lydian,
            Mode.Mixolydian,
            Mode.Aeolian,
            Mode.Locrian
        };

        // act
        var actualConfigurableScaleModes = _compositionConfigurationService.ConfigurableScaleModes;

        // assert
        actualConfigurableScaleModes.Should().BeEquivalentTo(expectedConfigurableScaleModes);
    }

    [Test]
    public void ConfigurableMeters_returns_expected_values()
    {
        // arrange
        var expectedConfigurableMeters = new[]
        {
            Meter.FourFour,
            Meter.ThreeFour
        };

        // act
        var actualConfigurableMeters = _compositionConfigurationService.ConfigurableMeters;

        // assert
        actualConfigurableMeters.Should().BeEquivalentTo(expectedConfigurableMeters);
    }

    [Test]
    public void ConfigureDefaults_dispatches_expected_actions()
    {
        // act
        _compositionConfigurationService.ConfigureDefaults();

        // assert
        _mockOrnamentationConfigurationService.Received(1).ConfigureDefaults();
        _mockCompositionRuleConfigurationService.Received(1).ConfigureDefaults();
        _mockInstrumentConfigurationService.Received(1).ConfigureDefaults();
        _mockDispatcher.Received(1).Dispatch(Arg.Any<UpdateCompositionConfiguration>());
    }

    [Test]
    public void Randomize_dispatches_expected_update()
    {
        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<UpdateCompositionConfiguration>());
    }
}
