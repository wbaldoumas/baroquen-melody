using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="ITonicizationApplicator"/>
/// <remarks>
///     The one chromatic gesture the diatonic walk cannot express: a minor triad standing a perfect fifth
///     above the chord it approaches raises its third by a semitone, becoming that chord's true dominant -
///     in Aeolian, v-i gains the raised leading tone of an authentic cadence, and i-iv / iv-VII tonicize
///     the subdominant and the relative-major's dominant. Eligibility is judged on the diatonic walk
///     (chord numbers, pre-alteration), the raise happens after the walk, and the raised third's voice
///     must already step up a whole tone into the target's root - the register-precise leading-tone
///     resolution - so the walk is provably unchanged and every alteration resolves by construction.
///     Raising is all-or-nothing per site: every doubling of the third raises together or the site is
///     rejected (a participant inside a suspension figure rejects the site, which also prevents a held
///     natural third sounding against a raised one). Ornamentation on the raised voice moves with it -
///     sub-notes matching the third's pitch class are raised, and when the diatonic step below the third
///     is a whole step, sub-notes on that lower neighbor are raised too, keeping figures like the
///     cadential trill free of augmented seconds - while another voice's ornament that sounds a natural
///     against any raised pitch class is a false relation and yields its decoration. The final measure
///     never functions as a dominant, only as a target: the crafted closing cadence stays unaltered
///     inside, and the exposition pass at completion can never re-draw a site in the boundary measure
///     borrowed from the body. Runs after the suspension pass and before the sustain pass, in the body
///     pipeline and again over the prepended theme exposition at completion.
/// </remarks>
internal sealed class TonicizationApplicator(
    IChordNumberIdentifier chordNumberIdentifier,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    CompositionConfiguration compositionConfiguration
) : ITonicizationApplicator
{
    private const int WholeStep = 2;

    private static readonly FrozenDictionary<ChordNumber, ChordNumber> DominantsByTarget = new Dictionary<ChordNumber, ChordNumber>
    {
        { ChordNumber.I, ChordNumber.V },
        { ChordNumber.II, ChordNumber.VI },
        { ChordNumber.III, ChordNumber.VII },
        { ChordNumber.IV, ChordNumber.I },
        { ChordNumber.V, ChordNumber.II },
        { ChordNumber.VI, ChordNumber.III },
        { ChordNumber.VII, ChordNumber.IV }
    }.ToFrozenDictionary();

    private readonly TonicizationConfiguration _tonicizationConfiguration =
        compositionConfiguration.TonicizationConfiguration ?? TonicizationConfiguration.Default;

    // In Aeolian the minor triads are i, iv, and v; their thirds and their targets' roots, resolved once
    // from the scale's degrees. Other modes gain their own tables when the mode gate lifts.
    private readonly FrozenDictionary<ChordNumber, NoteName> _thirdsByDominant = new Dictionary<ChordNumber, NoteName>
    {
        { ChordNumber.I, compositionConfiguration.Scale.Mediant },
        { ChordNumber.IV, compositionConfiguration.Scale.Submediant },
        { ChordNumber.V, compositionConfiguration.Scale.LeadingTone }
    }.ToFrozenDictionary();

    public void ApplyTonicization(Composition composition)
    {
        if (!_tonicizationConfiguration.Enabled || compositionConfiguration.Mode != Mode.Aeolian)
        {
            return;
        }

        // The final measure is never a dominant, only a target: its crafted cadence stays unaltered
        // inside, and in the exposition pass the boundary measure borrowed from the already-tonicized
        // body is only ever read, so no site can be drawn twice across the two passes.
        for (var measureIndex = 0; measureIndex < composition.Measures.Count - 1; measureIndex++)
        {
            var measure = composition.Measures[measureIndex];

            // Dominants land immediately before the strong slots: mid-measure (slot 2, approached by
            // slot 1)...
            if (measure.Beats.Count > 2)
            {
                TryApplyTonicization(measure.Beats[1], measure.Beats[2]);
            }

            // ...and the next downbeat, approached by this measure's final beat. Unlike the suspension
            // pass, the pair into the final measure is included: the closing cadence is the flagship
            // site, and only the approaching chord is ever altered.
            TryApplyTonicization(measure.Beats[^1], composition.Measures[measureIndex + 1].Beats[0]);
        }
    }

    private static bool IsMinorTriad(ChordNumber chordNumber) => chordNumber
        is ChordNumber.I
        or ChordNumber.IV
        or ChordNumber.V;

    private static bool IsInsideSuspensionFigure(BaroquenNote note) => note.OrnamentationType
        is OrnamentationType.Suspension
        or OrnamentationType.SuspensionResolution;

    private void TryApplyTonicization(Beat dominantBeat, Beat targetBeat)
    {
        var dominantNumber = chordNumberIdentifier.IdentifyChordNumber(dominantBeat.Chord);
        var targetNumber = chordNumberIdentifier.IdentifyChordNumber(targetBeat.Chord);

        // An unidentifiable target rejects through the lookup: Unknown is never a key in the table.
        if (!DominantsByTarget.TryGetValue(targetNumber, out var dominantOfTarget) ||
            dominantNumber != dominantOfTarget ||
            !IsMinorTriad(dominantNumber))
        {
            return;
        }

        var thirdNoteName = _thirdsByDominant[dominantNumber];
        var participants = dominantBeat.Chord.Notes.Where(note => note.NoteName == thirdNoteName).ToList();

        if (participants.Count == 0)
        {
            return;
        }

        // All-or-nothing: every doubling of the third must be free to raise and obligated to step up a
        // whole tone, or the whole site is rejected - a partial raise would sound the natural and raised
        // third simultaneously. In every licensed pair the target's root sits exactly a whole step above
        // the third, so the interval check both lands the voice on the target's root and pins the
        // register: after the raise, the leading tone resolves up by half step, never to a root in some
        // other octave.
        foreach (var participant in participants)
        {
            if (IsInsideSuspensionFigure(participant) ||
                !targetBeat.Chord.ContainsInstrument(participant.Instrument) ||
                targetBeat.Chord[participant.Instrument].Raw.NoteNumber - participant.Raw.NoteNumber != WholeStep ||
                !compositionConfiguration.IsNoteInInstrumentRange(participant.Instrument, RaisedNote(participant.Raw)))
            {
                return;
            }
        }

        if (!weightedRandomBooleanGenerator.IsTrue(_tonicizationConfiguration.Probability))
        {
            return;
        }

        NoteName? raisedNeighborNoteName = null;

        foreach (var participant in participants)
        {
            raisedNeighborNoteName = Raise(participant, thirdNoteName) ?? raisedNeighborNoteName;
        }

        foreach (var bystander in dominantBeat.Chord.Notes.Where(note => note.NoteName != thirdNoteName))
        {
            // An ornament sounding a natural against a raised pitch class - the third always, the
            // courtesy neighbor when some participant's figure actually raised it - is a false relation;
            // the decoration yields to the structural harmony, the same precedence suspensions take.
            // Notes inside suspension figures are untouchable here by construction: stamping a
            // suspension empties both notes' ornamentation lists, so they can never match.
            if (bystander.Ornamentations.Any(ornamentation => ornamentation.NoteName == thirdNoteName || ornamentation.NoteName == raisedNeighborNoteName))
            {
                bystander.ResetOrnamentation(compositionConfiguration.DefaultNoteTimeSpan);
            }
        }
    }

    /// <summary>
    ///     Raise the participant's principal note and the sub-notes of its figures that move with it,
    ///     returning the courtesy neighbor's natural pitch class when the figure actually raised it, so
    ///     the caller can guard other voices against sounding that natural as a false relation.
    /// </summary>
    private NoteName? Raise(BaroquenNote participant, NoteName thirdNoteName)
    {
        var lowerNeighborNoteName = LowerNeighborRequiringCourtesy(participant);
        NoteName? raisedNeighborNoteName = null;

        foreach (var ornamentation in participant.Ornamentations)
        {
            if (ornamentation.NoteName == thirdNoteName)
            {
                ornamentation.Alter(RaisedNote(ornamentation.Raw));
            }
            else if (lowerNeighborNoteName is not null && ornamentation.NoteName == lowerNeighborNoteName)
            {
                ornamentation.Alter(RaisedNote(ornamentation.Raw));
                raisedNeighborNoteName = lowerNeighborNoteName;
            }
        }

        participant.Alter(RaisedNote(participant.Raw));

        return raisedNeighborNoteName;
    }

    /// <summary>
    ///     When the diatonic step below the raised third is a whole step, leaving it natural would put an
    ///     augmented second inside the raised voice's own figures (the classic ♭6-♯7 gap), so sub-notes on
    ///     that neighbor are raised along with the third - the melodic-minor courtesy. When the step below
    ///     is already a half step, no courtesy is needed. Must run before the participant is altered: a
    ///     raised note is no longer in the scale and would have no index.
    /// </summary>
    private NoteName? LowerNeighborRequiringCourtesy(BaroquenNote participant)
    {
        var scaleNotes = compositionConfiguration.Scale.GetNotes();
        var thirdScaleIndex = compositionConfiguration.Scale.IndexOf(participant);

        // At the scale's floor note there is no lower neighbor to consult.
        if (thirdScaleIndex <= 0)
        {
            return null;
        }

        var lowerNeighbor = scaleNotes[thirdScaleIndex - 1];

        return participant.NoteNumber - lowerNeighbor.NoteNumber == 2 ? lowerNeighbor.NoteName : null;
    }

    private static Note RaisedNote(Note note) => Note.Get((SevenBitNumber)(note.NoteNumber + 1));
}
