using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="IChordInversionIdentifier"/>
/// <remarks>
///     The bass is the lowest-register instrument voiced in the chord (per the register-ordered
///     <see cref="CompositionConfiguration.Instruments"/>), matching how the voice-leading rules define the
///     lowest voice, rather than the literal lowest sounding pitch under voice crossing.
/// </remarks>
internal sealed class ChordInversionIdentifier(
    IChordNumberIdentifier chordNumberIdentifier,
    CompositionConfiguration compositionConfiguration
) : IChordInversionIdentifier
{
    public ChordInversion IdentifyChordInversion(BaroquenChord chord)
    {
        var chordNumber = chordNumberIdentifier.IdentifyChordNumber(chord);

        if (ChordTriad.FromChordNumber(compositionConfiguration.Scale, chordNumber) is not { } chordTriad)
        {
            return ChordInversion.Unknown;
        }

        var instruments = compositionConfiguration.Instruments;

        for (var instrumentIndex = instruments.Count - 1; instrumentIndex >= 0; --instrumentIndex)
        {
            if (!chord.ContainsInstrument(instruments[instrumentIndex]))
            {
                continue;
            }

            return ToChordInversion(chord[instruments[instrumentIndex]].NoteName, chordTriad);
        }

        return ChordInversion.Unknown;
    }

    private static ChordInversion ToChordInversion(NoteName bassNoteName, ChordTriad chordTriad)
    {
        if (bassNoteName == chordTriad.Root)
        {
            return ChordInversion.RootPosition;
        }

        if (bassNoteName == chordTriad.Third)
        {
            return ChordInversion.FirstInversion;
        }

        return bassNoteName == chordTriad.Fifth ? ChordInversion.SecondInversion : ChordInversion.Unknown;
    }
}
