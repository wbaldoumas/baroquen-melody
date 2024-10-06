namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateBaroquenMelody(MidiFileComposition MidiFileComposition, string Path, bool HasBeenSaved);
