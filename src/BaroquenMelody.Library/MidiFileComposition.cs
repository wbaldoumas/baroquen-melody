﻿using Melanchall.DryWetMidi.Core;

namespace BaroquenMelody.Library;

/// <summary>
///     A Baroquen melody.
/// </summary>
/// <param name="MidiFile">The MIDI file representing the Baroquen melody.</param>
public sealed record MidiFileComposition(MidiFile MidiFile);
