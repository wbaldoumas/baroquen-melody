using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Midi;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composers;

[TestFixture]
internal sealed class MidiFileComposerTests
{
    private IComposer _mockComposer = null!;

    private IMidiGenerator _mockMidiGenerator = null!;

    private MidiFileComposer _midiFileComposer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockComposer = Substitute.For<IComposer>();
        _mockMidiGenerator = Substitute.For<IMidiGenerator>();

        _midiFileComposer = new MidiFileComposer(_mockComposer, _mockMidiGenerator);
    }

    [Test]
    public void Compose_invokes_expected_components_and_returns_BaroquenMelody()
    {
        // arrange
        var testComposition = new Composition([]);
        var midiFile = new MidiFile();

        _mockComposer.Compose(CancellationToken.None).Returns(testComposition);
        _mockMidiGenerator.Generate(testComposition).Returns(midiFile);

        // act
        var result = _midiFileComposer.Compose(CancellationToken.None);

        // assert
        _mockComposer.Received(1).Compose(CancellationToken.None);
        _mockMidiGenerator.Received(1).Generate(testComposition);

        result.Should().NotBeNull();
        result.MidiFile.Should().Be(midiFile);
    }
}
