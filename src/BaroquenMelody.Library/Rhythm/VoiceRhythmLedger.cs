using BaroquenMelody.Library.Domain;
using System.Runtime.CompilerServices;

namespace BaroquenMelody.Library.Rhythm;

/// <inheritdoc cref="IVoiceRhythmLedger"/>
/// <remarks>
///     Reference identity is the contract. Value equality would collide the equal-pitch repeats that are the
///     ledger's main population (and <see cref="BaroquenNote"/> deliberately throws from
///     <see cref="object.GetHashCode"/>), while reference identity is stable under every in-place mutation
///     the later passes perform and is deliberately not carried by deep copies — a phrase repetition's or
///     exposition's copied note has no entry, so copies take the standard behavior.
/// </remarks>
internal sealed class VoiceRhythmLedger : IVoiceRhythmLedger
{
    private readonly HashSet<BaroquenNote> _heldNotes = new(NoteReferenceComparer.Instance);

    private readonly HashSet<BaroquenNote> _floridNotes = new(NoteReferenceComparer.Instance);

    private readonly Dictionary<BaroquenNote, int> _divisionIntensities = new(NoteReferenceComparer.Instance);

    private readonly HashSet<BaroquenNote> _textureFigurationNotes = new(NoteReferenceComparer.Instance);

    public void Clear()
    {
        _heldNotes.Clear();
        _floridNotes.Clear();
        _divisionIntensities.Clear();
        _textureFigurationNotes.Clear();
    }

    public void RecordHeldNote(BaroquenNote note) => _heldNotes.Add(note);

    public void RecordFloridNote(BaroquenNote note) => _floridNotes.Add(note);

    public bool IsHeldNote(BaroquenNote note) => _heldNotes.Contains(note);

    public bool IsFloridNote(BaroquenNote note) => _floridNotes.Contains(note);

    public void RecordDivisionIntensity(BaroquenNote note, int intensity) => _divisionIntensities[note] = intensity;

    public bool TryGetDivisionIntensity(BaroquenNote note, out int intensity) => _divisionIntensities.TryGetValue(note, out intensity);

    public void RecordTextureFigurationNote(BaroquenNote note) => _textureFigurationNotes.Add(note);

    public bool IsTextureFigurationNote(BaroquenNote note) => _textureFigurationNotes.Contains(note);

    private sealed class NoteReferenceComparer : IEqualityComparer<BaroquenNote>
    {
        public static NoteReferenceComparer Instance { get; } = new();

        public bool Equals(BaroquenNote? x, BaroquenNote? y) => ReferenceEquals(x, y);

        public int GetHashCode(BaroquenNote obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
