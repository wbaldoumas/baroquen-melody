using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="ICadenceClassifier"/>
/// <remarks>
///     Classification keys on chord numbers rather than chord qualities: the generator only produces diatonic
///     pitches, so in modes without a raised leading tone (Aeolian, Dorian, Phrygian) the diatonic minor v still
///     counts as V. Repeated chord numbers form no cadence.
/// </remarks>
internal sealed class CadenceClassifier(
    IChordNumberIdentifier chordNumberIdentifier,
    IChordInversionIdentifier chordInversionIdentifier,
    CompositionConfiguration compositionConfiguration
) : ICadenceClassifier
{
    private const int SemitonesInHalfStep = 1;

    public CadenceType ClassifyCadence(BaroquenChord penultimateChord, BaroquenChord finalChord)
    {
        var penultimateChordNumber = chordNumberIdentifier.IdentifyChordNumber(penultimateChord);
        var finalChordNumber = chordNumberIdentifier.IdentifyChordNumber(finalChord);

        if (penultimateChordNumber == ChordNumber.Unknown || finalChordNumber == ChordNumber.Unknown || penultimateChordNumber == finalChordNumber)
        {
            return CadenceType.None;
        }

        return (penultimateChordNumber, finalChordNumber) switch
        {
            (ChordNumber.V, ChordNumber.I) => ClassifyAuthenticCadence(penultimateChord, finalChord),
            (ChordNumber.IV, ChordNumber.I) => CadenceType.Plagal,
            (ChordNumber.V, ChordNumber.VI) => CadenceType.Deceptive,
            (_, ChordNumber.V) => ClassifyHalfCadence(penultimateChordNumber, penultimateChord, finalChord),
            _ => CadenceType.None
        };
    }

    private CadenceType ClassifyAuthenticCadence(BaroquenChord penultimateChord, BaroquenChord finalChord)
    {
        var isPerfect = chordInversionIdentifier.IdentifyChordInversion(penultimateChord) == ChordInversion.RootPosition
            && chordInversionIdentifier.IdentifyChordInversion(finalChord) == ChordInversion.RootPosition
            && finalChord.GetHighestVoicedNote(compositionConfiguration.Instruments)?.NoteName == compositionConfiguration.Scale.Tonic;

        return isPerfect ? CadenceType.PerfectAuthentic : CadenceType.ImperfectAuthentic;
    }

    private CadenceType ClassifyHalfCadence(ChordNumber penultimateChordNumber, BaroquenChord penultimateChord, BaroquenChord finalChord)
    {
        var isPhrygian = penultimateChordNumber == ChordNumber.IV
            && chordInversionIdentifier.IdentifyChordInversion(penultimateChord) == ChordInversion.FirstInversion
            && penultimateChord.GetLowestVoicedNote(compositionConfiguration.Instruments) is { } penultimateBassNote
            && finalChord.GetLowestVoicedNote(compositionConfiguration.Instruments) is { } finalBassNote
            && (int)penultimateBassNote.NoteNumber - (int)finalBassNote.NoteNumber == SemitonesInHalfStep;

        return isPhrygian ? CadenceType.PhrygianHalf : CadenceType.Half;
    }
}
