using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Forms.Enums;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <inheritdoc cref="IGroundBassFeasibilityAnalyzer"/>
/// <remarks>
///     The ground goes to the lowest voice (the smallest low bound, mirroring
///     <see cref="CompositionConfiguration.Instruments"/>). A pattern is feasible when some tonic-pitch-class
///     scale note anchors every offset inside that voice's range; among feasible anchors the one nearest the
///     range's center wins (ties toward the lower register, where a ground sits naturally). The scan draws
///     nothing from the random stream, so the UI can re-run it on every configuration change without
///     perturbing seeded compositions.
/// </remarks>
public sealed class GroundBassFeasibilityAnalyzer : IGroundBassFeasibilityAnalyzer
{
    public int GroundBassBankSize => GroundBassPattern.Bank.Count;

    public IReadOnlyList<GroundBass> GetFeasibleGroundBasses(CompositionConfiguration compositionConfiguration) => GetFeasibleGroundBasses(
        compositionConfiguration.InstrumentConfigurations,
        compositionConfiguration.Scale
    );

    public IReadOnlyList<GroundBass> GetFeasibleGroundBasses(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale) =>
        GetFeasibleRenderedGrounds(instrumentConfigurations, scale)
            .Select(static feasibleGround => feasibleGround.Pattern.Identifier)
            .ToList();

    /// <summary>
    ///     Retrieves the feasible patterns with their rendered bass notes, in bank order. The planner draws
    ///     its pattern from this list by index, so the order here fixes the meaning of the draw.
    /// </summary>
    /// <param name="instrumentConfigurations">The instrument configurations whose lowest voice hosts the ground.</param>
    /// <param name="scale">The scale supplying the tonic anchors and rendered notes.</param>
    /// <returns>The feasible patterns and their rendered bass notes, in bank order.</returns>
    internal static List<(GroundBassPattern Pattern, IReadOnlyList<Note> BassNotes)> GetFeasibleRenderedGrounds(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale)
    {
        var feasibleGrounds = new List<(GroundBassPattern Pattern, IReadOnlyList<Note> BassNotes)>();
        var bassConfiguration = instrumentConfigurations
            .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
            .LastOrDefault();

        if (bassConfiguration is null)
        {
            return feasibleGrounds;
        }

        foreach (var pattern in GroundBassPattern.Bank)
        {
            if (RenderPattern(pattern, scale, bassConfiguration) is { } bassNotes)
            {
                feasibleGrounds.Add((pattern, bassNotes));
            }
        }

        return feasibleGrounds;
    }

    private static List<Note>? RenderPattern(GroundBassPattern pattern, BaroquenScale scale, InstrumentConfiguration bassConfiguration)
    {
        var scaleNotes = scale.GetNotes();
        var tonicNoteName = scale.Tonic;
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

    // Bank offsets never exceed the anchor (a pinned pattern invariant), so only the low end of the scale's
    // note list can run out from under a rendered offset.
    private static bool AnchorFits(GroundBassPattern pattern, List<Note> scaleNotes, int anchorIndex, InstrumentConfiguration bassConfiguration) =>
        pattern.ScaleStepOffsets.All(offset =>
        {
            var renderedIndex = anchorIndex + offset;

            return renderedIndex >= 0 && bassConfiguration.IsNoteWithinInstrumentRange(scaleNotes[renderedIndex]);
        });
}
