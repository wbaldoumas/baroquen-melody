﻿using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <inheritdoc cref="IChordComposer"/>
internal sealed class ChordComposer(ICompositionStrategy compositionStrategy, CompositionConfiguration compositionConfiguration) : IChordComposer
{
    public BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var possibleChordChoices = compositionStrategy.GetPossibleChordChoices(precedingChords);
        var chordChoice = possibleChordChoices.OrderBy(_ => ThreadLocalRandom.Next()).First();

        return precedingChords[^1].ApplyChordChoice(compositionConfiguration.Scale, chordChoice);
    }
}
