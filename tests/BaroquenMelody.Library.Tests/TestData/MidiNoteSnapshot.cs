namespace BaroquenMelody.Library.Tests.TestData;

/// <summary>
///     A minimal, comparable projection of a MIDI note (the musically meaningful fields) used by the determinism,
///     invariant, and golden-master tests.
/// </summary>
/// <param name="NoteNumber">The MIDI note number.</param>
/// <param name="Velocity">The MIDI velocity.</param>
/// <param name="Time">The absolute start time, in MIDI ticks.</param>
/// <param name="Length">The duration, in MIDI ticks.</param>
internal readonly record struct MidiNoteSnapshot(int NoteNumber, int Velocity, long Time, long Length);
