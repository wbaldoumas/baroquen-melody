using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration;

[TestFixture]
internal sealed class CompositionConfigurationTests
{
    private const byte MinSopranoPitch = 60;

    private const byte MaxSopranoPitch = 72;

    private CompositionConfiguration _compositionConfiguration = null!;

    [SetUp]
    public void SetUp()
    {
        _compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, MinSopranoPitch.ToNote(), MaxSopranoPitch.ToNote()),
                new(Instrument.Two, 48.ToNote(), 60.ToNote()),
                new(Instrument.Three, 36.ToNote(), 48.ToNote()),
                new(Instrument.Four, 24.ToNote(), 36.ToNote())
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Aeolian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );
    }

    [Test]
    [TestCase(Instrument.One, MaxSopranoPitch + 1, false)]
    [TestCase(Instrument.One, MinSopranoPitch - 1, false)]
    [TestCase(Instrument.One, MaxSopranoPitch, true)]
    [TestCase(Instrument.One, MinSopranoPitch, true)]
    [TestCase(Instrument.One, MaxSopranoPitch - 1, true)]
    [TestCase(Instrument.One, MinSopranoPitch + 1, true)]
    public void IsNoteInInstrumentRange_returns_expected_result(
        Instrument instrument,
        byte pitch,
        bool expectedNoteIsInInstrumentRange)
    {
        var isNoteInInstrumentRange = _compositionConfiguration.IsNoteInInstrumentRange(instrument, pitch.ToNote());

        isNoteInInstrumentRange.Should().Be(expectedNoteIsInInstrumentRange);
    }
}
