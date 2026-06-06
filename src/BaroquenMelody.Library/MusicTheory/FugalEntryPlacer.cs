using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="IFugalEntryPlacer"/>
/// <remarks>
///     Placement is always a whole-octave transposition, so the entry's pitch classes - and therefore its identity as
///     the subject or answer - are preserved. The octave that leaves the fewest notes (principal and ornamentation)
///     outside the target instrument's range is chosen. Ties are broken toward the most centered placement, and then
///     toward the lower placement. When the entry spans a wider range than the instrument, the least-spilling octave is
///     still chosen and any spilling notes are left at their true pitch rather than clamped.
/// </remarks>
internal sealed class FugalEntryPlacer(CompositionConfiguration compositionConfiguration) : IFugalEntryPlacer
{
    private const int DiatonicNotesPerOctave = 7;

    public IReadOnlyList<BaroquenNote> Place(IReadOnlyList<BaroquenNote> entry, Instrument targetInstrument)
    {
        if (entry.Count == 0)
        {
            return [];
        }

        var notes = compositionConfiguration.Scale.GetNotes();
        var targetConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[targetInstrument];

        var minNoteNumber = (int)targetConfiguration.MinNote.NoteNumber;
        var maxNoteNumber = (int)targetConfiguration.MaxNote.NoteNumber;

        var entryNoteIndices = GetEntryNoteIndices(entry, notes);
        var octaveShift = ResolveBestOctaveShift(entryNoteIndices, notes, minNoteNumber, maxNoteNumber);

        return entry.Select(note => Shift(note, octaveShift, notes, targetInstrument)).ToList();
    }

    private static List<int> GetEntryNoteIndices(IReadOnlyList<BaroquenNote> entry, List<Note> notes)
    {
        var indices = new List<int>();

        foreach (var note in entry)
        {
            indices.Add(notes.IndexOf(note.Raw));

            foreach (var ornamentation in note.Ornamentations)
            {
                indices.Add(notes.IndexOf(ornamentation.Raw));
            }
        }

        return indices;
    }

    private static int ResolveBestOctaveShift(List<int> entryNoteIndices, List<Note> notes, int minNoteNumber, int maxNoteNumber)
    {
        var lowestNoteIndex = entryNoteIndices.Min();
        var highestNoteIndex = entryNoteIndices.Max();

        // Only octave shifts that keep every note representable within the scale's note list are considered.
        var lowestShift = -(lowestNoteIndex / DiatonicNotesPerOctave * DiatonicNotesPerOctave);
        var highestShift = (notes.Count - 1 - highestNoteIndex) / DiatonicNotesPerOctave * DiatonicNotesPerOctave;

        // The range center, doubled, so the comparison stays exact without integer-division rounding.
        var doubledVoiceCenter = minNoteNumber + maxNoteNumber;

        var bestShift = 0;
        var bestSpillCount = int.MaxValue;
        var bestCenterDistance = int.MaxValue;

        for (var shift = lowestShift; shift <= highestShift; shift += DiatonicNotesPerOctave)
        {
            var spillCount = 0;
            var lowestPlacedNoteNumber = int.MaxValue;
            var highestPlacedNoteNumber = int.MinValue;

            foreach (var noteIndex in entryNoteIndices)
            {
                var noteNumber = (int)notes[noteIndex + shift].NoteNumber;

                if (noteNumber < minNoteNumber || noteNumber > maxNoteNumber)
                {
                    spillCount++;
                }

                lowestPlacedNoteNumber = Math.Min(lowestPlacedNoteNumber, noteNumber);
                highestPlacedNoteNumber = Math.Max(highestPlacedNoteNumber, noteNumber);
            }

            var centerDistance = Math.Abs(lowestPlacedNoteNumber + highestPlacedNoteNumber - doubledVoiceCenter);

            // Fewer spilled notes wins; ties go to the more centered placement, then (iterating low to high) the lower one.
            if (spillCount > bestSpillCount || (spillCount == bestSpillCount && centerDistance >= bestCenterDistance))
            {
                continue;
            }

            bestShift = shift;
            bestSpillCount = spillCount;
            bestCenterDistance = centerDistance;
        }

        return bestShift;
    }

    private static BaroquenNote Shift(BaroquenNote note, int octaveShift, List<Note> notes, Instrument targetInstrument)
    {
        var placedNote = new BaroquenNote(targetInstrument, notes[notes.IndexOf(note.Raw) + octaveShift], note.MusicalTimeSpan)
        {
            OrnamentationType = note.OrnamentationType
        };

        foreach (var ornamentation in note.Ornamentations)
        {
            placedNote.Ornamentations.Add(new BaroquenNote(targetInstrument, notes[notes.IndexOf(ornamentation.Raw) + octaveShift], ornamentation.MusicalTimeSpan)
            {
                OrnamentationType = ornamentation.OrnamentationType
            });
        }

        return placedNote;
    }
}
