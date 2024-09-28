using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;

internal sealed record NoteIndexPair(int PrimaryNoteIndex, int SecondaryNoteIndex, int Pulse)
{
    private static readonly FrozenSet<int> StrongPulses = new[] { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48 }.ToFrozenSet();

    public bool OccursOnStrongPulse { get; } = StrongPulses.Contains(Pulse);
}
