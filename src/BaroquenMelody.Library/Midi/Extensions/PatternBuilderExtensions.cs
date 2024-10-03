﻿using BaroquenMelody.Library.Domain;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Midi.Extensions;

/// <summary>
///     A home for extension methods for <see cref="PatternBuilder"/>.
/// </summary>
internal static class PatternBuilderExtensions
{
    /// <summary>
    ///     Add a <see cref="BaroquenNote"/> and its ornamentations to the <see cref="PatternBuilder"/>.
    /// </summary>
    /// <param name="patternBuilder">The <see cref="PatternBuilder"/> to add the <see cref="BaroquenNote"/> to.</param>
    /// <param name="note">The <see cref="BaroquenNote"/> to add to the <see cref="PatternBuilder"/>.</param>
    public static void AddNote(this PatternBuilder patternBuilder, BaroquenNote note)
    {
        patternBuilder
            .SetNoteLength(note.MusicalTimeSpan)
            .SetVelocity(note.Velocity)
            .Note(note.Raw);

        foreach (var ornamentation in note.Ornamentations)
        {
            patternBuilder.AddNote(ornamentation);
        }
    }

    /// <summary>
    ///     Add a rest to the <see cref="PatternBuilder"/>.
    /// </summary>
    /// <param name="patternBuilder">The <see cref="PatternBuilder"/> to add the rest to.</param>
    /// <param name="length">The length of the rest to add to the <see cref="PatternBuilder"/>.</param>
    public static void AddRest(this PatternBuilder patternBuilder, MusicalTimeSpan length) => patternBuilder.StepForward(length);
}
