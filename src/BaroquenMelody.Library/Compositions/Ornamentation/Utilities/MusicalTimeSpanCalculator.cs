using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

internal sealed class MusicalTimeSpanCalculator : IMusicalTimeSpanCalculator
{
    public MusicalTimeSpan CalculatePrimaryNoteTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, null)
    };

    public MusicalTimeSpan CalculateOrnamentationTimeSpan(OrnamentationType ornamentationType, Meter meter) => ornamentationType switch
    {
        OrnamentationType.PassingTone when meter == Meter.FourFour => MusicalTimeSpan.Eighth,
        _ => throw new ArgumentOutOfRangeException(nameof(ornamentationType), ornamentationType, null)
    };
}
