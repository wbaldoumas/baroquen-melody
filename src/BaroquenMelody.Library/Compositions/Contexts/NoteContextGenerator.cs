using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <inheritdoc cref="INoteContextGenerator"/>
internal sealed class NoteContextGenerator : INoteContextGenerator
{
    public ISet<NoteContext> GenerateNoteContexts(VoiceConfiguration voiceConfiguration)
    {
        var pitches = Enumerable.Range(
            voiceConfiguration.MinPitch,
            voiceConfiguration.MaxPitch - voiceConfiguration.MinPitch + 1
        ).Select(pitch => (byte)pitch).ToList();

        var noteMotions = new List<NoteMotion> { NoteMotion.Ascending, NoteMotion.Descending };
        var noteMotionSpans = new List<NoteSpan> { NoteSpan.Step, NoteSpan.Leap };

        return pitches.SelectMany(
            pitch => noteMotions.SelectMany(
                noteMotion => noteMotionSpans.Select(
                    noteMotionSpan => new NoteContext(
                        voiceConfiguration.Voice,
                        pitch,
                        noteMotion,
                        noteMotionSpan
                    )
                )
            )
        ).Concat(
            pitches.Select(
                pitch => new NoteContext(
                    voiceConfiguration.Voice,
                    pitch,
                    NoteMotion.Oblique,
                    NoteSpan.None
                )
            )
        ).Where(voiceContext =>
            !(voiceContext.Pitch == voiceConfiguration.MaxPitch && voiceContext.NoteMotion == NoteMotion.Descending)
            && !(voiceContext.Pitch == voiceConfiguration.MinPitch && voiceContext.NoteMotion == NoteMotion.Ascending)
        ).ToHashSet();
    }
}
