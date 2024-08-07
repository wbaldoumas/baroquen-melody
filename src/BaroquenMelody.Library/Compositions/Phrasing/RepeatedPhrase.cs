﻿using BaroquenMelody.Library.Compositions.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Phrasing;

[ExcludeFromCodeCoverage]
internal sealed class RepeatedPhrase
{
    public int RepetitionCount { get; set; }

    public required List<Measure> Phrase { get; init; }
}
