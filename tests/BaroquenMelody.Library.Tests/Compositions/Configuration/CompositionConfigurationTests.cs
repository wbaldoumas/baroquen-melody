using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration;

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
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, MinSopranoPitch.ToNote(), MaxSopranoPitch.ToNote()),
                new(Voice.Alto, 48.ToNote(), 60.ToNote()),
                new(Voice.Tenor, 36.ToNote(), 48.ToNote()),
                new(Voice.Bass, 24.ToNote(), 36.ToNote())
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            MusicalTimeSpan.Half,
            CompositionLength: 100
        );
    }

    [Test]
    [TestCase(Voice.Soprano, MaxSopranoPitch + 1, false)]
    [TestCase(Voice.Soprano, MinSopranoPitch - 1, false)]
    [TestCase(Voice.Soprano, MaxSopranoPitch, true)]
    [TestCase(Voice.Soprano, MinSopranoPitch, true)]
    [TestCase(Voice.Soprano, MaxSopranoPitch - 1, true)]
    [TestCase(Voice.Soprano, MinSopranoPitch + 1, true)]
    public void IsPitchInVoiceRange_returns_expected_result(
        Voice voice,
        byte pitch,
        bool expectedPitchIsInVoiceRange)
    {
        var isPitchInVoiceRange = _compositionConfiguration.IsNoteInVoiceRange(voice, pitch.ToNote());

        isPitchInVoiceRange.Should().Be(expectedPitchIsInVoiceRange);
    }
}
