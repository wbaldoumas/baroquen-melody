﻿namespace BaroquenMelody.Library.Domain;

/// <summary>
///     Represents a composition of music.
/// </summary>
/// <param name="Measures">The measures that make up the composition.</param>
internal sealed record Composition(List<Measure> Measures);
