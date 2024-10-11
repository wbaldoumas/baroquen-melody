using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
internal sealed class BaroquenMelodyComposerConfiguratorTests
{
    private BaroquenMelodyComposerConfigurator _baroquenMelodyComposerConfigurator = null!;

    [SetUp]
    public void SetUp() => _baroquenMelodyComposerConfigurator = new BaroquenMelodyComposerConfigurator(Substitute.For<ILogger<MidiFileComposition>>(), Substitute.For<IDispatcher>());

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Configure_returns_configured_MidiFileComposer_which_can_compose_a_MidiFileComposition(CompositionConfiguration compositionConfiguration)
    {
        // arrange
        var midiFileComposer = _baroquenMelodyComposerConfigurator.Configure(compositionConfiguration);

        // act
        var midiFileComposition = midiFileComposer.Compose(CancellationToken.None);

        // assert
        midiFileComposition.Should().NotBeNull();
        midiFileComposition.MidiFile.Should().NotBeNull();
    }

    private static IEnumerable<TestCaseData> TestCases =>
        from numberOfInstruments in Enumerable.Range(1, 3)
        from meter in EnumUtils<Meter>.AsEnumerable()
        from mode in EnumUtils<Mode>.AsEnumerable()
        select new TestCaseData(TestCompositionConfigurations.Get(numberOfInstruments, 10) with { Meter = meter, Mode = mode });
}
