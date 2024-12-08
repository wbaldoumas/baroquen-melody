using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Extensions;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Enums.Extensions;
using BaroquenMelody.Library.Ornamentation.Utilities;
using Melanchall.DryWetMidi.Interaction;
using System.Collections;

namespace BaroquenMelody.Library.MusicTheory;

internal sealed class NoteOnsetCalculator(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration compositionConfiguration
) : INoteOnsetCalculator
{
    private static readonly MusicalTimeSpan Resolution = MusicalTimeSpan.ThirtySecond;

    public BitArray CalculateNoteOnsets(OrnamentationType ornamentationType)
    {
        var bitArray = InitializeNoteOnsetBitArray(compositionConfiguration.Meter);
        var primaryNoteDuration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, compositionConfiguration.Meter);
        var ornamentationCount = ornamentationType.OrnamentationCount();
        var noteOnsetCursor = primaryNoteDuration.DivideBy(Resolution);

        for (var ornamentationStep = 0; ornamentationStep < ornamentationCount; ornamentationStep++)
        {
            var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, compositionConfiguration.Meter, ornamentationStep);
            var cursorProgressionValue = ornamentationDuration.DivideBy(Resolution);

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
            Meter.FiveEight => new BitArray(20),
            Meter.ThreeFour => new BitArray(24),
            _ => throw new ArgumentOutOfRangeException(nameof(meter), meter, "Unsupported meter for note onset calculation.")
        };

        bitArray[0] = true; // the first note is always sounded on the first pulse

        return bitArray;
    }
}
