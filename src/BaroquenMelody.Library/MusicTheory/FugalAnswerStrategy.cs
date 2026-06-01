using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="IFugalAnswerStrategy"/>
/// <remarks>
///     The answer is the subject transposed up a diatonic fifth (a real answer). While the subject's opening notes
///     stay on the tonic-dominant axis (its "head"), a dominant (5th degree) is instead answered a fourth higher so it
///     lands on the tonic - flattening the opening interval to keep the answer in key (a tonal answer). Once the head
///     leaves the tonic-dominant axis, every remaining note is answered strictly up a fifth.
/// </remarks>
internal sealed class FugalAnswerStrategy(CompositionConfiguration compositionConfiguration) : IFugalAnswerStrategy
{
    private const int DiatonicFifth = 4;

    private const int DiatonicFourth = 3;

    public IReadOnlyList<BaroquenNote> GenerateAnswer(IReadOnlyList<BaroquenNote> subject)
    {
        var scale = compositionConfiguration.Scale;
        var notes = scale.GetNotes();

        var answer = new List<BaroquenNote>(subject.Count);
        var isWithinHead = true;

        foreach (var subjectNote in subject)
        {
            var isTonicOrDominant = subjectNote.NoteName == scale.Tonic || subjectNote.NoteName == scale.Dominant;

            if (!isTonicOrDominant)
            {
                isWithinHead = false;
            }

            var transposition = isWithinHead && subjectNote.NoteName == scale.Dominant ? DiatonicFourth : DiatonicFifth;

            answer.Add(Transpose(subjectNote, transposition, notes));
        }

        return answer;
    }

    private static BaroquenNote Transpose(BaroquenNote note, int scaleSteps, List<Note> notes)
    {
        var transposedNote = new BaroquenNote(note.Instrument, Shift(note, scaleSteps, notes), note.MusicalTimeSpan)
        {
            OrnamentationType = note.OrnamentationType
        };

        foreach (var ornamentation in note.Ornamentations)
        {
            transposedNote.Ornamentations.Add(new BaroquenNote(ornamentation.Instrument, Shift(ornamentation, scaleSteps, notes), ornamentation.MusicalTimeSpan)
            {
                OrnamentationType = ornamentation.OrnamentationType
            });
        }

        return transposedNote;
    }

    private static Note Shift(BaroquenNote note, int scaleSteps, List<Note> notes)
    {
        var shiftedIndex = Math.Clamp(notes.IndexOf(note.Raw) + scaleSteps, 0, notes.Count - 1);

        return notes[shiftedIndex];
    }
}
