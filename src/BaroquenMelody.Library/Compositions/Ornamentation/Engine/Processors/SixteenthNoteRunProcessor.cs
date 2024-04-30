using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class SixteenthNoteRunProcessor(IMusicalTimeSpanCalculator musicalTimeSpanCalculator, CompositionConfiguration configuration) : IProcessor<OrnamentationItem>
{
    public const int Interval = 4;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = configuration.Scale.GetNotes().ToList();

        var currentNoteScaleIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteScaleIndex = notes.IndexOf(nextNote.Raw);

        var firstSixteenthNoteIndex = currentNoteScaleIndex > nextNoteScaleIndex ? currentNoteScaleIndex - 1 : currentNoteScaleIndex + 1;
        var secondSixteenthNoteIndex = currentNoteScaleIndex > nextNoteScaleIndex ? currentNoteScaleIndex - 2 : currentNoteScaleIndex + 2;
        var thirdSixteenthNoteIndex = currentNoteScaleIndex > nextNoteScaleIndex ? currentNoteScaleIndex - 3 : currentNoteScaleIndex + 3;

        var firstSixteenthNote = notes[firstSixteenthNoteIndex];
        var secondSixteenthNote = notes[secondSixteenthNoteIndex];
        var thirdSixteenthNote = notes[thirdSixteenthNoteIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.SixteenthNoteRun, configuration.Meter);

        var duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.SixteenthNoteRun, configuration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, firstSixteenthNote)
        {
            Duration = duration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, secondSixteenthNote)
        {
            Duration = duration
        });

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, thirdSixteenthNote)
        {
            Duration = duration
        });
    }
}
