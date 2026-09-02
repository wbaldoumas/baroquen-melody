using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Viewpoints;

/// <summary>
///     Reads a chord context as a sequence of distinct harmonic events. The harmonic-rhythm hold and the ground
///     bass's held slots place plain duplicates of the preceding chord into the context, and a harmony sustained
///     across such duplicated beats is one event, not two - a scoring rule that indexed raw chord positions would
///     read every post-hold move as starting from a zero motion and sit inert across most of the body.
/// </summary>
/// <remarks>
///     A duplicate is a chord in which every voice repeats its predecessor's sounded pitch. Comparing raw pitches
///     rather than full note values keeps a hold recognizable after the ornamentation passes have decorated the
///     paired beats independently (the ground's close searches over decorated statements). A freshly composed
///     chord that re-strikes every pitch of its predecessor is indistinguishable from a hold here, and reads the
///     same way deliberately: an exact re-strike is melodically a repetition of the same harmonic event - a
///     repeat neither recovers a leap nor advances a root motion - so the collapse reads it the way the scoring
///     rules already intend.
/// </remarks>
internal static class HarmonicEvents
{
    /// <summary>
    ///     Retrieves the chord some number of distinct harmonic events back from the end of the context.
    /// </summary>
    /// <param name="precedingChords">The preceding chords, oldest first.</param>
    /// <param name="eventsBack">How many events back to look; one is the most recent event.</param>
    /// <returns>The chord opening that event, or <see langword="null"/> when the context holds too few events.</returns>
    public static BaroquenChord? PrecedingEventChord(IReadOnlyList<BaroquenChord> precedingChords, int eventsBack)
    {
        if (eventsBack <= 0)
        {
            return null;
        }

        var remainingEvents = eventsBack;

        for (var index = precedingChords.Count - 1; index >= 0; --index)
        {
            // A chord that sustains its older neighbor belongs to that neighbor's event, so a run of any
            // length counts once, at the chord that opened it.
            if (index > 0 && SustainsPrecedingChord(precedingChords[index], precedingChords[index - 1]))
            {
                continue;
            }

            if (--remainingEvents == 0)
            {
                return precedingChords[index];
            }
        }

        return null;
    }

    private static bool SustainsPrecedingChord(BaroquenChord chord, BaroquenChord precedingChord)
    {
        if (chord.Notes.Count != precedingChord.Notes.Count)
        {
            return false;
        }

        foreach (var note in chord.Notes)
        {
            if (!precedingChord.ContainsInstrument(note.Instrument) || precedingChord[note.Instrument].Raw != note.Raw)
            {
                return false;
            }
        }

        return true;
    }
}
