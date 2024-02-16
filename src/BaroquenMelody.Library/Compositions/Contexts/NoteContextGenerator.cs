using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="INoteContextGenerator"/>
internal sealed class NoteContextGenerator : INoteContextGenerator
{
    public ISet<NoteContext> GenerateNoteContexts(VoiceConfiguration voiceConfiguration, Scale scale)
    {
        var notes = scale.GetNotes().Where(voiceConfiguration.IsNoteWithinVoiceRange).ToList();
        var noteMotions = new List<NoteMotion> { NoteMotion.Ascending, NoteMotion.Descending };
        var noteMotionSpans = new List<NoteSpan> { NoteSpan.Step, NoteSpan.Leap };

        return notes.SelectMany(
            note => noteMotions.SelectMany(
                noteMotion => noteMotionSpans.Select(
                    noteMotionSpan => new NoteContext(
                        voiceConfiguration.Voice,
                        note,
                        noteMotion,
                        noteMotionSpan
                    )
                )
            )
        ).Concat(
            notes.Select(
                note => new NoteContext(
                    voiceConfiguration.Voice,
                    note,
                    NoteMotion.Oblique,
                    NoteSpan.None
                )
            )
        ).Where(voiceContext =>
            !(voiceContext.Note == voiceConfiguration.MaxNote && voiceContext.NoteMotion == NoteMotion.Descending)
            && !(voiceContext.Note == voiceConfiguration.MinNote && voiceContext.NoteMotion == NoteMotion.Ascending)
        ).ToHashSet();
    }
}
