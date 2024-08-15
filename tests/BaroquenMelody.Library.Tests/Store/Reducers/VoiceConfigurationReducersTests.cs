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

        state.Configurations[Voice.One].MinNote.Should().Be(Notes.C7);
        state.Configurations[Voice.One].MaxNote.Should().Be(Notes.C8);
        state.Configurations[Voice.One].Instrument.Should().Be(GeneralMidi2Program.Dulcimer);
        state.Configurations[Voice.One].IsEnabled.Should().BeFalse();

        state.Configurations[Voice.Two].MinNote.Should().Be(Notes.C5);
        state.Configurations[Voice.Two].MaxNote.Should().Be(Notes.C6);
        state.Configurations[Voice.Two].Instrument.Should().Be(GeneralMidi2Program.Banjo);
        state.Configurations[Voice.Two].IsEnabled.Should().BeTrue();

        state.Configurations[Voice.Three].MinNote.Should().Be(Notes.C6);
        state.Configurations[Voice.Three].MaxNote.Should().Be(Notes.C7);
        state.Configurations[Voice.Three].Instrument.Should().Be(GeneralMidi2Program.Celesta);
        state.Configurations[Voice.Three].IsEnabled.Should().BeTrue();
    }
}
