using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
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
        _compositionConfiguration = new CompositionConfiguration(new HashSet<VoiceConfiguration>
        {
            new(Voice.Soprano, MinSopranoPitch, MaxSopranoPitch),
            new(Voice.Alto, 48, 60),
            new(Voice.Tenor, 36, 48),
            new(Voice.Bass, 24, 36)
        });
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
        var isPitchInVoiceRange = _compositionConfiguration.IsPitchInVoiceRange(voice, pitch);

        isPitchInVoiceRange.Should().Be(expectedPitchIsInVoiceRange);
    }
}
