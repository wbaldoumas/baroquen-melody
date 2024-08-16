﻿using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

[TestFixture]
internal sealed class InstrumentConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private InstrumentConfigurationService _instrumentConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();

        _instrumentConfigurationService = new InstrumentConfigurationService(_mockDispatcher);
    }

    [Test]
    public void ConfigurableInstruments_returns_expected_values()
    {
        // arrange
        var expectedConfigurableInstruments = new[] { Instrument.One, Instrument.Two, Instrument.Three, Instrument.Four };

        // act
        var actualConfigurableInstruments = _instrumentConfigurationService.ConfigurableInstruments;

        // assert
        actualConfigurableInstruments.Should().BeEquivalentTo(expectedConfigurableInstruments);
    }

    [Test]
    public void ConfigureDefaults_dispatches_expected_actions()
    {
        // act
        _instrumentConfigurationService.ConfigureDefaults();

        // assert
        _mockDispatcher.Received(4).Dispatch(Arg.Any<UpdateInstrumentConfiguration>());
    }
}
