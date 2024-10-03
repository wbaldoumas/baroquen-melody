using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.State;

[TestFixture]
internal sealed class InstrumentConfigurationStateTests
{
    [Test]
    [TestCase(ConfigurationStatus.Enabled, ConfigurationStatus.Enabled, true)]
    [TestCase(ConfigurationStatus.Enabled, ConfigurationStatus.Locked, true)]
    [TestCase(ConfigurationStatus.Locked | ConfigurationStatus.Enabled, ConfigurationStatus.Enabled, true)]
    [TestCase(ConfigurationStatus.Enabled, ConfigurationStatus.Disabled, true)]
    [TestCase(ConfigurationStatus.Disabled, ConfigurationStatus.Disabled, false)]
    [TestCase(ConfigurationStatus.Disabled, ConfigurationStatus.Locked, false)]
    [TestCase(ConfigurationStatus.Disabled, ConfigurationStatus.EnabledAndLocked, true)]
    [TestCase(ConfigurationStatus.Disabled, ConfigurationStatus.DisabledAndLocked, false)]
    public void IsValid_returns_expected_value(ConfigurationStatus instrumentOneStatus, ConfigurationStatus instrumentTwoStatus, bool expectedIsValid)
    {
        // act
        var state = new InstrumentConfigurationState(
            new Dictionary<Instrument, InstrumentConfiguration>
            {
                {
                    Instrument.One,
                    new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, instrumentOneStatus)
                },
                {
                    Instrument.Two,
                    new InstrumentConfiguration(Instrument.Two, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, instrumentTwoStatus)
                }
            },
            new Dictionary<Instrument, InstrumentConfiguration>()
        );

        // assert
        state.IsValid.Should().Be(expectedIsValid);

        if (!expectedIsValid)
        {
            state.ValidationMessage.Should().NotBeNullOrEmpty();
        }
        else
        {
            state.ValidationMessage.Should().BeNullOrEmpty();
        }
    }

    [Test]
    public void AllConfigurations_returns_expected_configurations()
    {
        // act
        var state = new InstrumentConfigurationState(
            new Dictionary<Instrument, InstrumentConfiguration>
            {
                {
                    Instrument.One,
                    new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, ConfigurationStatus.Enabled)
                },
                {
                    Instrument.Two,
                    new InstrumentConfiguration(Instrument.Two, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, ConfigurationStatus.Enabled)
                }
            },
            new Dictionary<Instrument, InstrumentConfiguration>()
        );

        // assert
        state.AllConfigurations.Should().BeEquivalentTo(
            [
                new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, ConfigurationStatus.Enabled),
                new InstrumentConfiguration(Instrument.Two, Notes.C4, Notes.C5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.Dulcimer, ConfigurationStatus.Enabled)
            ]
        );
    }
}
