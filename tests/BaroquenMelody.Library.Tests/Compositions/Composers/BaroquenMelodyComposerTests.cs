using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Midi;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Composers;

[TestFixture]
internal sealed class BaroquenMelodyComposerTests
{
    private IComposer _mockComposer = null!;

    private IMidiGenerator _mockMidiGenerator = null!;

    private BaroquenMelodyComposer _baroquenMelodyComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockComposer = Substitute.For<IComposer>();
        _mockMidiGenerator = Substitute.For<IMidiGenerator>();

        _baroquenMelodyComposer = new BaroquenMelodyComposer(_mockComposer, _mockMidiGenerator);
    }

    [Test]
    public void Compose_invokes_expected_components_and_returns_BaroquenMelody()
    {
        // arrange
        var testComposition = new Composition([]);
        var midiFile = new MidiFile();

        _mockComposer.Compose().Returns(testComposition);
        _mockMidiGenerator.Generate(testComposition).Returns(midiFile);

        // act
        var result = _baroquenMelodyComposer.Compose();

        // assert
        _mockComposer.Received(1).Compose();
        _mockMidiGenerator.Received(1).Generate(testComposition);

        result.Should().NotBeNull();
        result.MidiFile.Should().Be(midiFile);
    }
}
