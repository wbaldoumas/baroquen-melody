using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Extensions;
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

        if (chord.GetLowestVoicedNote(compositionConfiguration.Instruments) is not { } bassNote)
        {
            return ChordInversion.Unknown;
        }

        return ToChordInversion(bassNote.NoteName, chordTriad);
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
