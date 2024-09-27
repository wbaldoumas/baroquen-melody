using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums.Extensions;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

internal sealed class NoteOnsetCalculator(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : INoteOnsetCalculator
{
    private static readonly MusicalTimeSpan _resolution = MusicalTimeSpan.ThirtySecond;

    public BitArray CalculateNoteOnsets(OrnamentationType ornamentationType)
    {
        var bitArray = InitializeNoteOnsetBitArray(compositionConfiguration.Meter);
        var primaryNoteDuration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        var ornamentationCount = ornamentationType.OrnamentationCount();
        var noteOnsetCursor = primaryNoteDuration.DivideBy(_resolution);

        for (var ornamentationStep = 1; ornamentationStep <= ornamentationCount; ornamentationStep++)
        {
            var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter, ornamentationStep);
            var cursorProgressionValue = ornamentationDuration.DivideBy(_resolution);

            bitArray[noteOnsetCursor] = true;

            noteOnsetCursor += cursorProgressionValue;
        }

        return bitArray;
    }

    private static BitArray InitializeNoteOnsetBitArray(Meter meter)
    {
        var bitArray = meter switch
        {
            Meter.FourFour => new BitArray(16),
            Meter.ThreeFour => new BitArray(24),
            _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Unsupported meter for note onset calculation.")
        };

        bitArray[0] = true; // the first note is always sounded on the first pulse

        return bitArray;
    }
}
