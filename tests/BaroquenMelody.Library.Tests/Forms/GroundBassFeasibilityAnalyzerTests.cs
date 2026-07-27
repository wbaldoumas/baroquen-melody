using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using Mode = BaroquenMelody.Library.MusicTheory.Enums.Mode;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Forms;

/// <summary>
///     Pins the feasibility analyzer the planner and the UI warnings both consume. The range fixtures mirror
///     the planner's, and the tonic-and-mode sweeps promote the default-range design claims - the enabled
///     defaults host the whole bank in every key, and the four-voice defaults lose only the octave-spanning
///     romanesca at the bass ceiling's two highest tonics - into permanent invariants.
/// </summary>
[TestFixture]
internal sealed class GroundBassFeasibilityAnalyzerTests
{
    private GroundBassFeasibilityAnalyzer _analyzer = null!;

    [SetUp]
    public void SetUp() => _analyzer = new GroundBassFeasibilityAnalyzer();

    [Test]
    public void GroundBassBankSize_ReportsTheBuiltInBank()
    {
        _analyzer.GroundBassBankSize.Should().Be(GroundBassPattern.Bank.Count);
        _analyzer.GroundBassBankSize.Should().Be(3, "the UI's 'N of 3 grounds fit' copy leans on the bank size");
    }

    [Test]
    public void GetFeasibleGroundBasses_WithTheTestShapedBassRange_ReturnsTheWholeBankInBankOrder()
    {
        // arrange: the test-shaped C2-C3 bass hosts every pattern, and the configuration overload must see
        // exactly what the enumerable overload sees.
        var configuration = TestCompositionConfigurations.Get(3, 25);

        // act
        var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses(configuration);

        // assert
        feasibleGroundBasses.Should().Equal(GroundBass.DescendingTetrachord, GroundBass.Romanesca, GroundBass.CadentialGround);
        feasibleGroundBasses.Should().Equal(
            _analyzer.GetFeasibleGroundBasses(configuration.InstrumentConfigurations, configuration.Scale),
            "the configuration overload is a straight delegation"
        );
    }

    [Test]
    public void GetFeasibleGroundBasses_WithABassRangeTooNarrowBelowItsTonic_ReturnsOnlyTheTetrachord()
    {
        // arrange: a G3-B4 bass in C Ionian holds a single tonic (C4) with just three scale steps below it
        // in range, so the octave-descending grounds cannot anchor.
        var instrumentConfigurations = BuildTenorBassInstruments();

        // act
        var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses(instrumentConfigurations, new BaroquenScale(NoteName.C, Mode.Ionian));

        // assert
        feasibleGroundBasses.Should().Equal(GroundBass.DescendingTetrachord);
    }

    [Test]
    public void GetFeasibleGroundBasses_WhenNoPatternCanAnchorInTheBassRange_ReturnsEmpty()
    {
        // arrange: a B2-B3 bass contains a single tonic (C3) with only one scale step below it in range,
        // so no bank pattern can anchor - the configuration the UI warns about with the fugue-fallback chip.
        var instrumentConfigurations = BuildTwoVoiceInstruments(Notes.B2, Notes.B3);

        // act
        var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses(instrumentConfigurations, new BaroquenScale(NoteName.C, Mode.Ionian));

        // assert
        feasibleGroundBasses.Should().BeEmpty();
    }

    [Test]
    public void GetFeasibleGroundBasses_WithNoInstruments_ReturnsEmpty()
    {
        // arrange: the UI can observe a state with every voice disabled; there is no bass to host a ground.
        var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses([], new BaroquenScale(NoteName.C, Mode.Ionian));

        // assert
        feasibleGroundBasses.Should().BeEmpty();
    }

    [Test]
    public void GetFeasibleGroundBasses_WithTheDefaultEnabledVoices_HostsTheWholeBankForEveryTonicAndMode()
    {
        // arrange: the default bass (Three, C3-B4) spans two chromatic octaves, so every tonic pitch class
        // appears twice and the upper occurrence always carries a full scale octave beneath it.
        var enabledDefaults = InstrumentConfiguration.DefaultConfigurations.Values.Where(static configuration => configuration.IsEnabled).ToHashSet();

        foreach (var tonic in Enum.GetValues<NoteName>())
        {
            foreach (var mode in Enum.GetValues<Mode>())
            {
                // act
                var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses(enabledDefaults, new BaroquenScale(tonic, mode));

                // assert
                feasibleGroundBasses.Should().Equal(
                    [GroundBass.DescendingTetrachord, GroundBass.Romanesca, GroundBass.CadentialGround],
                    "the default enabled voices must host the whole bank in {0} {1}",
                    tonic,
                    mode
                );
            }
        }
    }

    [Test]
    public void GetFeasibleGroundBasses_WithEveryDefaultVoiceEnabled_LosesOnlyTheRomanescaAtTheBassCeilingTonics()
    {
        // arrange: with the fourth voice enabled the C2-A3 bass hosts the ground; its A3 ceiling excludes
        // the upper tonic octave for A-sharp and B, and only the octave-spanning romanesca needs it.
        var allDefaults = InstrumentConfiguration.DefaultConfigurations.Values
            .Select(static configuration => configuration with { Status = ConfigurationStatus.Enabled })
            .ToHashSet();

        foreach (var tonic in Enum.GetValues<NoteName>())
        {
            foreach (var mode in Enum.GetValues<Mode>())
            {
                // act
                var feasibleGroundBasses = _analyzer.GetFeasibleGroundBasses(allDefaults, new BaroquenScale(tonic, mode));

                // assert
                IReadOnlyList<GroundBass> expectedGroundBasses = tonic is NoteName.ASharp or NoteName.B
                    ? [GroundBass.DescendingTetrachord, GroundBass.CadentialGround]
                    : [GroundBass.DescendingTetrachord, GroundBass.Romanesca, GroundBass.CadentialGround];

                feasibleGroundBasses.Should().Equal(
                    expectedGroundBasses,
                    "the four-voice defaults keep every ground but the romanesca at the bass ceiling in {0} {1}",
                    tonic,
                    mode
                );
            }
        }
    }

    private static HashSet<InstrumentConfiguration> BuildTenorBassInstruments() =>
    [
        new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Two, Notes.E4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Three, Notes.G3, Notes.B4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
    ];

    private static HashSet<InstrumentConfiguration> BuildTwoVoiceInstruments(Note bassMinNote, Note bassMaxNote) =>
    [
        new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Two, bassMinNote, bassMaxNote, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
    ];
}
