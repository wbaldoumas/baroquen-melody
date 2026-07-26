using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;
using System.Runtime.InteropServices;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.MusicTheory;

/// <inheritdoc cref="ITonicizationApplicator"/>
/// <remarks>
///     The one chromatic gesture the diatonic walk cannot express: a minor triad standing a perfect fifth
///     above the chord it approaches raises its third by a semitone, becoming that chord's true dominant.
///     The licenses derive from the mode itself - every minor triad is a potential dominant of the chord
///     a fifth below - so in Aeolian v-i gains the raised leading tone of an authentic cadence and
///     i-iv / iv-VII tonicize the subdominant and the relative-major's dominant, while in Ionian ii-V
///     becomes the classical V/V and iii-vi / vi-ii tonicize the relative minor and the supertonic. The
///     mode gate lifts mode by mode (Ionian and Aeolian today): the remaining modes derive license
///     tables just as cleanly, but their ficta await their own review. Eligibility is judged on the diatonic walk
///     (chord numbers, pre-alteration), the raise happens after the walk, and the raised third's voice
///     must already step up a whole tone into the target's root - the register-precise leading-tone
///     resolution - so the walk is provably unchanged and every alteration resolves by construction.
///     Raising is all-or-nothing per site: every doubling of the third raises together, with every
///     sub-note the raise will touch surviving in range, or the site is rejected (a participant inside
///     a suspension figure rejects the site too, which also prevents a held natural third sounding
///     against a raised one). Ornamentation is a participant in the raise, never a casualty: in every
///     voice's figure - the raised voice's own and the other voices' alike - sub-notes sounding the
///     third's pitch class raise with the harmony, so no natural third is left to sound against the
///     raised one as a false relation. When the diatonic step below the third is a whole step, a
///     sub-note on that neighbor stepping directly into a raised third raises too, closing the b6-#7
///     augmented second inside the figure, while the same pitch class a leap away or elsewhere in the
///     texture keeps its natural - the ascending-raised / descending-natural coexistence minor-mode
///     practice already licenses. The final measure
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

    private const int MinorThirdSemitones = 3;

    private const int MajorThirdSemitones = 4;

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

    // The tonicizable dominants, derived once from the mode's own degree qualities: a triad on a scale
    // degree is minor when its root-to-third spans three semitones and its third-to-fifth four, and
    // every minor triad becomes its target's true dominant by raising its third. The derivation also
    // resolves the courtesy per license: when the diatonic step below a dominant's third is a whole
    // step, a figure stepping between that neighbor and the raised third would sound the b6-#7
    // augmented second, so the neighbor raises along with it; a half-step neighbor needs no courtesy.
    // In Aeolian the minor triads are i, iv, and v (only v's subtonic third carries a courtesy); in
    // Ionian they are ii, iii, and vi (only iii's).
    private readonly FrozenDictionary<ChordNumber, TonicizableDominant> _tonicizableDominants =
        BuildTonicizableDominants(compositionConfiguration.Scale);

    public void ApplyTonicization(Composition composition)
    {
        // The gate lifts mode by mode: Ionian and Aeolian carry the classical licenses; the remaining
        // modes derive tables just as cleanly, but their ficta await their own musical review.
        if (!_tonicizationConfiguration.Enabled || compositionConfiguration.Mode is not Mode.Ionian and not Mode.Aeolian)
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

    private static bool IsInsideSuspensionFigure(BaroquenNote note) => note.OrnamentationType
        is OrnamentationType.Suspension
        or OrnamentationType.SuspensionResolution;

    private void TryApplyTonicization(Beat dominantBeat, Beat targetBeat)
    {
        var dominantNumber = chordNumberIdentifier.IdentifyChordNumber(dominantBeat.Chord);
        var targetNumber = chordNumberIdentifier.IdentifyChordNumber(targetBeat.Chord);

        // An unidentifiable target rejects through the first lookup: Unknown is never a key in the
        // table. A major or diminished source rejects through the second: only minor triads gain a
        // raise - the major triads already are dominants, and raising a diminished triad's third alone
        // would leave its tritone unresolved.
        if (!DominantsByTarget.TryGetValue(targetNumber, out var dominantOfTarget) ||
            dominantNumber != dominantOfTarget ||
            !_tonicizableDominants.TryGetValue(dominantNumber, out var tonicizableDominant))
        {
            return;
        }

        var thirdNoteName = tonicizableDominant.ThirdNoteName;
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

        // Every voice's figures participate in the raise - sub-notes sounding the third's pitch class in
        // any voice, plus the courtesy tones stepping into them - and each planned raise must survive in
        // range, the same all-or-nothing obligation the doubled thirds carry, or the site is rejected
        // with every figure intact.
        var plannedRaises = new List<BaroquenNote>();
        var courtesyNoteName = tonicizableDominant.CourtesyNoteName;

        foreach (var note in dominantBeat.Chord.Notes)
        {
            if (!TryPlanFigureRaises(note, thirdNoteName, courtesyNoteName, plannedRaises))
            {
                return;
            }
        }

        if (!weightedRandomBooleanGenerator.IsTrue(_tonicizationConfiguration.Probability))
        {
            return;
        }

        foreach (var participant in participants)
        {
            participant.Alter(RaisedNote(participant.Raw));
        }

        foreach (var ornamentation in plannedRaises)
        {
            ornamentation.Alter(RaisedNote(ornamentation.Raw));
        }
    }

    /// <summary>
    ///     Plan which of the voice's sub-notes the raise carries: every sub-note sounding the third's
    ///     pitch class, in any octave, and every courtesy-tone sub-note that steps directly into a raised
    ///     third - temporally adjacent in the figure's sounding order (principal first, then sub-notes)
    ///     and sitting exactly the whole step below, the geometry that would otherwise sound the
    ///     augmented second. A courtesy tone a leap away from the raised third keeps its natural: no
    ///     augmented second exists across a leap. Returns <see langword="false"/> when a planned raise
    ///     would leave the instrument's range, rejecting the site.
    /// </summary>
    private bool TryPlanFigureRaises(BaroquenNote note, NoteName thirdNoteName, NoteName? courtesyNoteName, List<BaroquenNote> plannedRaises)
    {
        var ornamentations = note.Ornamentations;
        var principalRaises = note.NoteName == thirdNoteName;
        var raises = new bool[ornamentations.Count];

        for (var index = 0; index < ornamentations.Count; index++)
        {
            raises[index] = ornamentations[index].NoteName == thirdNoteName;
        }

        for (var index = 0; index < ornamentations.Count; index++)
        {
            if (raises[index] || ornamentations[index].NoteName != courtesyNoteName)
            {
                continue;
            }

            // A courtesy raise can never enable another: the courtesy pitch class is a whole step below
            // the third, so two courtesy tones are never a whole step apart and the marks cannot cascade.
            // The principal counts as the first sub-note's predecessor even when a figure renders it
            // silently (the appoggiatura): the courtesy needs its predecessor a whole step above, and the
            // appoggiatura's first sub-note always sits above its anchor, so the shortcut cannot misfire.
            var precedingRaises = index == 0 ? principalRaises : raises[index - 1];
            var preceding = index == 0 ? note : ornamentations[index - 1];

            raises[index] =
                (precedingRaises && preceding.NoteNumber - ornamentations[index].NoteNumber == WholeStep) ||
                (index + 1 < ornamentations.Count && raises[index + 1] && ornamentations[index + 1].NoteNumber - ornamentations[index].NoteNumber == WholeStep);
        }

        for (var index = 0; index < ornamentations.Count; index++)
        {
            if (!raises[index])
            {
                continue;
            }

            if (!compositionConfiguration.IsNoteInInstrumentRange(note.Instrument, RaisedNote(ornamentations[index].Raw)))
            {
                return false;
            }

            plannedRaises.Add(ornamentations[index]);
        }

        return true;
    }

    private static FrozenDictionary<ChordNumber, TonicizableDominant> BuildTonicizableDominants(BaroquenScale scale)
    {
        NoteName[] degrees = [scale.Tonic, scale.Supertonic, scale.Mediant, scale.Subdominant, scale.Dominant, scale.Submediant, scale.LeadingTone];
        ChordNumber[] chordNumbers = [ChordNumber.I, ChordNumber.II, ChordNumber.III, ChordNumber.IV, ChordNumber.V, ChordNumber.VI, ChordNumber.VII];

        var tonicizableDominants = new Dictionary<ChordNumber, TonicizableDominant>();

        for (var degreeIndex = 0; degreeIndex < degrees.Length; degreeIndex++)
        {
            var triad = ChordTriad.FromChordNumber(scale, chordNumbers[degreeIndex])!.Value;

            if (SemitonesUp(triad.Root, triad.Third) != MinorThirdSemitones || SemitonesUp(triad.Third, triad.Fifth) != MajorThirdSemitones)
            {
                continue;
            }

            // The diatonic step below a triad's third is the degree between its root and its third.
            var lowerNeighborNoteName = degrees[(degreeIndex + 1) % degrees.Length];

            tonicizableDominants.Add(chordNumbers[degreeIndex], new TonicizableDominant(triad.Third, CourtesyNoteName(triad.Third, lowerNeighborNoteName)));
        }

        return tonicizableDominants.ToFrozenDictionary();
    }

    private static NoteName? CourtesyNoteName(NoteName thirdNoteName, NoteName lowerNeighborNoteName) =>
        SemitonesUp(lowerNeighborNoteName, thirdNoteName) == WholeStep ? lowerNeighborNoteName : null;

    private static int SemitonesUp(NoteName lowerNoteName, NoteName upperNoteName) => ((int)upperNoteName - (int)lowerNoteName + 12) % 12;

    private static Note RaisedNote(Note note) => Note.Get((SevenBitNumber)(note.NoteNumber + 1));

    /// <summary>
    ///     A minor triad's identity as a potential dominant: the third its raise sharpens, and the
    ///     whole-step lower neighbor obligated to raise beside it, when the mode gives it one.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct TonicizableDominant(NoteName ThirdNoteName, NoteName? CourtesyNoteName);
}
