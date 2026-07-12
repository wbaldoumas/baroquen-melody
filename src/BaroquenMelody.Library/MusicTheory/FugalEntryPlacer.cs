using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using System.Collections.Frozen;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="IFugalEntryPlacer"/>
/// <remarks>
///     Placement is always a whole-octave transposition, so the entry's pitch classes - and therefore its identity as
///     the subject or answer - are preserved. The octave leaving the fewest principal notes unviable is chosen: a
///     principal note is viable when it lies within the target instrument's range and, when the voice spacing rule
///     constrains the instrument, within its spacing-feasible notes. The exposition validates its pinned entry
///     chords without the spacing rule, so spacing-infeasible placements are not fatal - steering into the feasible
///     registers keeps the theme aligned with the spacing the rest of the composition enforces. Ties are broken
///     toward the fewest notes (principal and ornamentation) outside the instrument's range, then the most centered
///     placement, then the lower placement. When no fully viable placement exists, the least-unviable octave is
///     still chosen and the notes are left at their true pitch rather than clamped.
///     <para>
///         Every entry note is assumed to be diatonic to the configured scale, as the subject and answer supplied by the
///         theme composer always are; a non-scale note would be a programming error.
///     </para>
/// </remarks>
/// <param name="compositionConfiguration">The composition configuration supplying the scale and instrument ranges.</param>
/// <param name="spacingFeasibleNoteNumbersByInstrument">
///     The spacing-feasible note numbers per instrument, or <see langword="null"/> when the voice spacing rule does
///     not constrain placement (disabled, auto-disabled, or fewer than three voices leave every playable note viable).
/// </param>
internal sealed class FugalEntryPlacer(
    CompositionConfiguration compositionConfiguration,
    FrozenDictionary<Instrument, FrozenSet<int>>? spacingFeasibleNoteNumbersByInstrument = null
) : IFugalEntryPlacer
{
    private const int DiatonicNotesPerOctave = 7;

    public IReadOnlyList<BaroquenNote> Place(IReadOnlyList<BaroquenNote> entry, Instrument targetInstrument, BaroquenNote? precedingNote = null)
    {
        if (entry.Count == 0)
        {
            return [];
        }

        var notes = compositionConfiguration.Scale.GetNotes();
        var targetConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[targetInstrument];

        var minNoteNumber = (int)targetConfiguration.MinNote.NoteNumber;
        var maxNoteNumber = (int)targetConfiguration.MaxNote.NoteNumber;
        var feasibleNoteNumbers = spacingFeasibleNoteNumbersByInstrument?.GetValueOrDefault(targetInstrument);
        var precedingNoteIndex = precedingNote is null ? -1 : notes.IndexOf(precedingNote.Raw);

        var principalNoteIndices = GetPrincipalNoteIndices(entry, notes);
        var ornamentationNoteIndices = GetOrnamentationNoteIndices(entry, notes);
        var octaveShift = ResolveBestOctaveShift(principalNoteIndices, ornamentationNoteIndices, notes, minNoteNumber, maxNoteNumber, feasibleNoteNumbers, precedingNoteIndex);

        return entry.Select(note => Shift(note, octaveShift, notes, targetInstrument)).ToList();
    }

    private static List<int> GetPrincipalNoteIndices(IReadOnlyList<BaroquenNote> entry, List<Note> notes) =>
        entry.Select(note => notes.IndexOf(note.Raw)).ToList();

    private static List<int> GetOrnamentationNoteIndices(IReadOnlyList<BaroquenNote> entry, List<Note> notes) =>
        entry.SelectMany(static note => note.Ornamentations).Select(ornamentation => notes.IndexOf(ornamentation.Raw)).ToList();

    private static int ResolveBestOctaveShift(
        List<int> principalNoteIndices,
        List<int> ornamentationNoteIndices,
        List<Note> notes,
        int minNoteNumber,
        int maxNoteNumber,
        FrozenSet<int>? feasibleNoteNumbers,
        int precedingNoteIndex)
    {
        var lowestNoteIndex = principalNoteIndices.Concat(ornamentationNoteIndices).Min();
        var highestNoteIndex = principalNoteIndices.Concat(ornamentationNoteIndices).Max();

        // Only octave shifts that keep every note representable within the scale's note list are considered.
        var lowestShift = -(lowestNoteIndex / DiatonicNotesPerOctave * DiatonicNotesPerOctave);
        var highestShift = (notes.Count - 1 - highestNoteIndex) / DiatonicNotesPerOctave * DiatonicNotesPerOctave;

        // The range center, doubled, so the comparison stays exact without integer-division rounding.
        var doubledVoiceCenter = minNoteNumber + maxNoteNumber;

        var bestShift = 0;
        var bestUnviablePrincipalCount = int.MaxValue;
        var bestSpillCount = int.MaxValue;
        var bestCenterDistance = int.MaxValue;

        for (var shift = lowestShift; shift <= highestShift; shift += DiatonicNotesPerOctave)
        {
            var unviablePrincipalCount = 0;
            var spillCount = 0;
            var lowestPlacedNoteNumber = int.MaxValue;
            var highestPlacedNoteNumber = int.MinValue;

            // An entry whose first note the voice cannot step to from where it currently sits is as fatal as an
            // unviable principal: the chord search only moves a voice by MaxScaleStepChange scale steps per beat.
            if (precedingNoteIndex >= 0 && Math.Abs(principalNoteIndices[0] + shift - precedingNoteIndex) > CompositionConfiguration.MaxScaleStepChange)
            {
                unviablePrincipalCount++;
            }

            foreach (var noteIndex in principalNoteIndices)
            {
                var noteNumber = (int)notes[noteIndex + shift].NoteNumber;
                var isSpilled = noteNumber < minNoteNumber || noteNumber > maxNoteNumber;

                if (isSpilled)
                {
                    spillCount++;
                }

                if (isSpilled || feasibleNoteNumbers?.Contains(noteNumber) == false)
                {
                    unviablePrincipalCount++;
                }

                lowestPlacedNoteNumber = Math.Min(lowestPlacedNoteNumber, noteNumber);
                highestPlacedNoteNumber = Math.Max(highestPlacedNoteNumber, noteNumber);
            }

            foreach (var noteIndex in ornamentationNoteIndices)
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

            // Fewer unviable principal notes wins; ties go to fewer spilled notes overall, then the more centered
            // placement, then (iterating low to high) the lower one.
            var improves = unviablePrincipalCount < bestUnviablePrincipalCount
                || (unviablePrincipalCount == bestUnviablePrincipalCount && spillCount < bestSpillCount)
                || (unviablePrincipalCount == bestUnviablePrincipalCount && spillCount == bestSpillCount && centerDistance < bestCenterDistance);

            if (!improves)
            {
                continue;
            }

            bestShift = shift;
            bestUnviablePrincipalCount = unviablePrincipalCount;
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
