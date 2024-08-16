using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class VoiceConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateVoiceConfiguration_updates_voice_configurations_as_expected()
    {
        // arrange
        var state = new VoiceConfigurationState();

        // act
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.One, Notes.C4, Notes.C5, GeneralMidi2Program.Accordion, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.One, Notes.C7, Notes.C8, GeneralMidi2Program.Dulcimer, false));

        // assert
        state.Configurations.Should().ContainKeys(Voice.One, Voice.Two, Voice.Three);

        state[Voice.One]!.MinNote.Should().Be(Notes.C7);
        state[Voice.One]!.MaxNote.Should().Be(Notes.C8);
        state[Voice.One]!.Instrument.Should().Be(GeneralMidi2Program.Dulcimer);
        state[Voice.One]!.IsEnabled.Should().BeFalse();
        state[Voice.Two]!.MinNote.Should().Be(Notes.C5);
        state[Voice.Two]!.MaxNote.Should().Be(Notes.C6);
        state[Voice.Two]!.Instrument.Should().Be(GeneralMidi2Program.Banjo);
        state[Voice.Two]!.IsEnabled.Should().BeTrue();
        state[Voice.Three]!.MinNote.Should().Be(Notes.C6);
        state[Voice.Three]!.MaxNote.Should().Be(Notes.C7);
        state[Voice.Three]!.Instrument.Should().Be(GeneralMidi2Program.Celesta);
        state[Voice.Three]!.IsEnabled.Should().BeTrue();

        state.Aggregate.Should().BeEquivalentTo(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo),
                new(Voice.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta)
            }
        );
    }
}
