using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="INoteTransposer"/>
internal sealed class NoteTransposer(CompositionConfiguration compositionConfiguration) : INoteTransposer
{
    public IEnumerable<BaroquenNote> TransposeToInstrument(IEnumerable<BaroquenNote> notesToTranspose, Instrument sourceInstrument, Instrument targetInstrument)
    {
        var sourceInstrumentConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[sourceInstrument];
        var targetInstrumentConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[targetInstrument];

        var notes = compositionConfiguration.Scale.GetNotes();

        var currentInstrumentMinNoteIndex = notes.IndexOf(sourceInstrumentConfiguration.MinNote);
        var currentInstrumentMaxNoteIndex = notes.IndexOf(sourceInstrumentConfiguration.MaxNote);
        var targetInstrumentMinNoteIndex = notes.IndexOf(targetInstrumentConfiguration.MinNote);
        var targetInstrumentMaxNoteIndex = notes.IndexOf(targetInstrumentConfiguration.MaxNote);

        var transposedNotes = new List<BaroquenNote>();

        foreach (var noteToTranspose in notesToTranspose)
        {
            var noteToTransposeNoteIndex = notes.IndexOf(noteToTranspose.Raw);
            var transposedNoteIndex = Transpose(currentInstrumentMinNoteIndex, currentInstrumentMaxNoteIndex, targetInstrumentMinNoteIndex, targetInstrumentMaxNoteIndex, noteToTransposeNoteIndex);

            var transposedNote = new BaroquenNote(targetInstrument, notes[transposedNoteIndex], noteToTranspose.MusicalTimeSpan)
            {
                OrnamentationType = noteToTranspose.OrnamentationType
            };

            foreach (var ornamentedNote in noteToTranspose.Ornamentations)
            {
                var ornamentedNoteIndex = notes.IndexOf(ornamentedNote.Raw);
                var transposedOrnamentedNoteIndex = Transpose(currentInstrumentMinNoteIndex, currentInstrumentMaxNoteIndex, targetInstrumentMinNoteIndex, targetInstrumentMaxNoteIndex, ornamentedNoteIndex);

                var newOrnamentedNote = new BaroquenNote(targetInstrument, notes[transposedOrnamentedNoteIndex], ornamentedNote.MusicalTimeSpan)
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
