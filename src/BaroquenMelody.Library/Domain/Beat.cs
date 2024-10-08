﻿using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Domain;

/// <summary>
///     Represents a beat in a composition.
/// </summary>
/// <param name="Chord"> The chord that is played during the beat. </param>
internal sealed record Beat(BaroquenChord Chord)
{
    public BaroquenNote this[Instrument instrument] => Chord[instrument];

    public bool ContainsInstrument(Instrument instrument) => Chord.ContainsInstrument(instrument);

    /// <summary>
    ///     Initializes a new instance of the <see cref="Beat"/> class.
    /// </summary>
    /// <param name="beat">The beat to copy.</param>
    public Beat(Beat beat) => Chord = new BaroquenChord(beat.Chord);
}
