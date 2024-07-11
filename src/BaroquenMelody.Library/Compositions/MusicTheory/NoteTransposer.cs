using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

internal sealed class NoteTransposer(CompositionConfiguration compositionConfiguration) : INoteTransposer
{
    public IEnumerable<BaroquenNote> TransposeToVoice(IEnumerable<BaroquenNote> notesToTranspose, Voice oldVoice, Voice newVoice)
    {
        var oldVoiceConfiguration = compositionConfiguration.VoiceConfigurationsByVoice[oldVoice];
        var newVoiceConfiguration = compositionConfiguration.VoiceConfigurationsByVoice[newVoice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var oldVoiceMin = notes.IndexOf(oldVoiceConfiguration.MinNote);
        var oldVoiceMax = notes.IndexOf(oldVoiceConfiguration.MaxNote);
        var newVoiceMin = notes.IndexOf(newVoiceConfiguration.MinNote);
        var newVoiceMax = notes.IndexOf(newVoiceConfiguration.MaxNote);

        var result = new List<BaroquenNote>();

        foreach (var note in notesToTranspose)
        {
            var noteIndex = notes.IndexOf(note.Raw);
            var newNoteIndex = Transpose(oldVoiceMin, oldVoiceMax, newVoiceMin, newVoiceMax, noteIndex);

            var newNote = new BaroquenNote(newVoice, notes[newNoteIndex])
            {
                OrnamentationType = note.OrnamentationType,
                Duration = note.Duration
            };

            foreach (var ornamentedNote in note.Ornamentations)
            {
                var ornamentedNoteIndex = notes.IndexOf(ornamentedNote.Raw);
                var newOrnamentedNoteIndex = Transpose(oldVoiceMin, oldVoiceMax, newVoiceMin, newVoiceMax, ornamentedNoteIndex);

                var newOrnamentedNote = new BaroquenNote(newVoice, notes[newOrnamentedNoteIndex])
                {
                    OrnamentationType = ornamentedNote.OrnamentationType,
                    Duration = ornamentedNote.Duration
                };

                newNote.Ornamentations.Add(newOrnamentedNote);
            }

            result.Add(newNote);
        }

        return result;
    }

    public static int Transpose(int oldMin, int oldMax, int newMin, int newMax, int numberToTranspose)
    {
        var oldMiddle = (oldMin + oldMax) / 2.0;
        var newMiddle = (newMin + newMax) / 2.0;

        var middleDifference = (int)Math.Round(newMiddle - oldMiddle, MidpointRounding.ToZero);

        return numberToTranspose + middleDifference;
    }
}
