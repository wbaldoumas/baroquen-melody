using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
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
    [TestCase(true, true, true)]
    [TestCase(true, false, false)]
    public void IsValid_returns_expected_value(bool isInstrumentOneEnabled, bool isInstrumentTwoEnabled, bool expectedIsValid)
    {
        // act
        var state = new InstrumentConfigurationState(
            new Dictionary<Instrument, InstrumentConfiguration>
            {
                {
                    Instrument.One,
                    new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, GeneralMidi2Program.Dulcimer, isInstrumentOneEnabled)
                },
                {
                    Instrument.Two,
                    new InstrumentConfiguration(Instrument.Two, Notes.C4, Notes.C5, GeneralMidi2Program.Dulcimer, isInstrumentTwoEnabled)
                },
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
}
