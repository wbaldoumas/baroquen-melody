﻿using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonance : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var notes = nextChord.Notes.Select(note => note.Raw).ToHashSet();

        return notes.All(note => notes.Where(otherNote => otherNote != note).All(otherNote => !note.IsDissonantWith(otherNote)));
    }
}
