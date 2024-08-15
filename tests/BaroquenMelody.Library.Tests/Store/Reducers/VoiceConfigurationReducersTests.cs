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
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Soprano, Notes.C4, Notes.C5, GeneralMidi2Program.Accordion, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Alto, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Tenor, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta, true));
        state = VoiceConfigurationReducers.ReduceUpdateVoiceConfiguration(state, new UpdateVoiceConfiguration(Voice.Soprano, Notes.C7, Notes.C8, GeneralMidi2Program.Dulcimer, false));

        // assert
        state.Configurations.Should().ContainKeys(Voice.Soprano, Voice.Alto, Voice.Tenor);

        state.Configurations[Voice.Soprano].MinNote.Should().Be(Notes.C7);
        state.Configurations[Voice.Soprano].MaxNote.Should().Be(Notes.C8);
        state.Configurations[Voice.Soprano].Instrument.Should().Be(GeneralMidi2Program.Dulcimer);
        state.Configurations[Voice.Soprano].IsEnabled.Should().BeFalse();

        state.Configurations[Voice.Alto].MinNote.Should().Be(Notes.C5);
        state.Configurations[Voice.Alto].MaxNote.Should().Be(Notes.C6);
        state.Configurations[Voice.Alto].Instrument.Should().Be(GeneralMidi2Program.Banjo);
        state.Configurations[Voice.Alto].IsEnabled.Should().BeTrue();

        state.Configurations[Voice.Tenor].MinNote.Should().Be(Notes.C6);
        state.Configurations[Voice.Tenor].MaxNote.Should().Be(Notes.C7);
        state.Configurations[Voice.Tenor].Instrument.Should().Be(GeneralMidi2Program.Celesta);
        state.Configurations[Voice.Tenor].IsEnabled.Should().BeTrue();
    }
}
