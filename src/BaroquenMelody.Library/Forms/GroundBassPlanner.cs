using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <inheritdoc cref="IGroundBassPlanner"/>
/// <remarks>
///     The ground goes to the lowest configured voice. A pattern is feasible when some tonic-pitch-class
///     scale note anchors every offset inside that voice's range; among feasible anchors the one nearest the
///     range's center wins (ties toward the lower register, where a ground sits naturally). One random draw
///     picks among the feasible patterns, so plan creation costs exactly one draw whenever any ground fits.
/// </remarks>
internal sealed class GroundBassPlanner(
    CompositionConfiguration compositionConfiguration,
    IRandomProvider randomProvider
) : IGroundBassPlanner
{
    /// <summary>
    ///     Every composition states the ground at least twice: the opening solo statement and one full-texture
    ///     statement, whatever the configured minimum measure count.
    /// </summary>
    private const int MinStatementCount = 2;

    public GroundBassPlan? CreatePlan()
    {
        var bassInstrument = compositionConfiguration.Instruments[^1];
        var feasiblePatterns = new List<(GroundBassPattern Pattern, IReadOnlyList<Note> BassNotes)>();

        foreach (var pattern in GroundBassPattern.Bank)
        {
            if (RenderPattern(pattern, bassInstrument) is { } bassNotes)
            {
                feasiblePatterns.Add((pattern, bassNotes));
            }
        }

        if (feasiblePatterns.Count == 0)
        {
            return null;
        }

        var (chosenPattern, chosenBassNotes) = feasiblePatterns[randomProvider.Next(feasiblePatterns.Count)];
        var measuresPerStatement = chosenPattern.ScaleStepOffsets.Count * GroundBassPlan.SlotsPerGroundNote / compositionConfiguration.BeatsPerMeasure;
        var statementCount = Math.Max(
            MinStatementCount,
            (compositionConfiguration.MinimumMeasures + measuresPerStatement - 1) / measuresPerStatement
        );

        return new GroundBassPlan(chosenPattern, bassInstrument, chosenBassNotes, statementCount, measuresPerStatement);
    }

    private List<Note>? RenderPattern(GroundBassPattern pattern, Instrument bassInstrument)
    {
        var scaleNotes = compositionConfiguration.Scale.GetNotes();
        var tonicNoteName = compositionConfiguration.Scale.Tonic;
        var bassConfiguration = compositionConfiguration.InstrumentConfigurationsByInstrument[bassInstrument];
        var rangeCenter = (bassConfiguration.MinNote.NoteNumber + bassConfiguration.MaxNote.NoteNumber) / 2;

        var bestAnchorIndex = -1;
        var bestDistance = int.MaxValue;

        for (var scaleIndex = 0; scaleIndex < scaleNotes.Count; ++scaleIndex)
        {
            if (scaleNotes[scaleIndex].NoteName != tonicNoteName || !AnchorFits(pattern, scaleNotes, scaleIndex, bassConfiguration))
            {
                continue;
            }

            var distance = Math.Abs(scaleNotes[scaleIndex].NoteNumber - rangeCenter);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestAnchorIndex = scaleIndex;
            }
        }

        return bestAnchorIndex < 0
            ? null
            : pattern.ScaleStepOffsets.Select(offset => scaleNotes[bestAnchorIndex + offset]).ToList();
    }

    private static bool AnchorFits(GroundBassPattern pattern, List<Note> scaleNotes, int anchorIndex, InstrumentConfiguration bassConfiguration) =>
        pattern.ScaleStepOffsets.All(offset =>
        {
            var renderedIndex = anchorIndex + offset;

            return renderedIndex >= 0 && renderedIndex < scaleNotes.Count && bassConfiguration.IsNoteWithinInstrumentRange(scaleNotes[renderedIndex]);
        });
}
