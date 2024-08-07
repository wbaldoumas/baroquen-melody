using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

/// <inheritdoc cref="INoteTransposer"/>
internal sealed class NoteTransposer(CompositionConfiguration compositionConfiguration) : INoteTransposer
{
    public IEnumerable<BaroquenNote> TransposeToVoice(IEnumerable<BaroquenNote> notesToTranspose, Voice currentVoice, Voice targetVoice)
    {
        var currentVoiceConfiguration = compositionConfiguration.VoiceConfigurationsByVoice[currentVoice];
        var targetVoiceConfiguration = compositionConfiguration.VoiceConfigurationsByVoice[targetVoice];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentVoiceMinNoteIndex = notes.IndexOf(currentVoiceConfiguration.MinNote);
        var currentVoiceMaxNoteIndex = notes.IndexOf(currentVoiceConfiguration.MaxNote);
        var targetVoiceMinNoteIndex = notes.IndexOf(targetVoiceConfiguration.MinNote);
        var targetVoiceMaxNoteIndex = notes.IndexOf(targetVoiceConfiguration.MaxNote);

        var transposedNotes = new List<BaroquenNote>();

        foreach (var noteToTranspose in notesToTranspose)
        {
            var noteToTransposeNoteIndex = notes.IndexOf(noteToTranspose.Raw);
            var transposedNoteIndex = Transpose(currentVoiceMinNoteIndex, currentVoiceMaxNoteIndex, targetVoiceMinNoteIndex, targetVoiceMaxNoteIndex, noteToTransposeNoteIndex);

            var transposedNote = new BaroquenNote(targetVoice, notes[transposedNoteIndex], noteToTranspose.MusicalTimeSpan)
            {
                OrnamentationType = noteToTranspose.OrnamentationType
            };

            foreach (var ornamentedNote in noteToTranspose.Ornamentations)
            {
                var ornamentedNoteIndex = notes.IndexOf(ornamentedNote.Raw);
                var transposedOrnamentedNoteIndex = Transpose(currentVoiceMinNoteIndex, currentVoiceMaxNoteIndex, targetVoiceMinNoteIndex, targetVoiceMaxNoteIndex, ornamentedNoteIndex);

                var newOrnamentedNote = new BaroquenNote(targetVoice, notes[transposedOrnamentedNoteIndex], ornamentedNote.MusicalTimeSpan)
                {
                    OrnamentationType = ornamentedNote.OrnamentationType
                };

                transposedNote.Ornamentations.Add(newOrnamentedNote);
            }

            transposedNotes.Add(transposedNote);
        }

        return transposedNotes;
    }

    public static int Transpose(int currentMin, int currentMax, int targetMin, int targetMax, int noteIndexToTranspose)
    {
        var oldMiddle = (currentMin + currentMax) / 2.0;
        var newMiddle = (targetMin + targetMax) / 2.0;

        var middleDifference = (int)Math.Round(newMiddle - oldMiddle, MidpointRounding.ToZero);

        return noteIndexToTranspose + middleDifference;
    }
}
