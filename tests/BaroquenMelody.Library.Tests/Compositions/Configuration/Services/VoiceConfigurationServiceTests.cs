using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

[TestFixture]
internal sealed class VoiceConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private VoiceConfigurationService _voiceConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();

        _voiceConfigurationService = new VoiceConfigurationService(_mockDispatcher);
    }

    [Test]
    public void ConfigurableVoices_returns_expected_values()
    {
        // arrange
        var expectedConfigurableVoices = new[] { Voice.One, Voice.Two, Voice.Three, Voice.Four };

        // act
        var actualConfigurableVoices = _voiceConfigurationService.ConfigurableVoices;

        // assert
        actualConfigurableVoices.Should().BeEquivalentTo(expectedConfigurableVoices);
    }

    [Test]
    public void ConfigureDefaults_dispatches_expected_actions()
    {
        // act
        _voiceConfigurationService.ConfigureDefaults();

        // assert
        _mockDispatcher.Received(4).Dispatch(Arg.Any<UpdateVoiceConfiguration>());
    }
}
