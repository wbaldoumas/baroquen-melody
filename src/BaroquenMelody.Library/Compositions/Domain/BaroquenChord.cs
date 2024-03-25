using BaroquenMelody.Library.Compositions.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Domain;

internal sealed class BaroquenChord(IEnumerable<BaroquenNote> notes)
{
    public IEnumerable<BaroquenNote> Notes => _notes.Values;

    public BaroquenNote this[Voice voice] => _notes[voice];

    private readonly FrozenDictionary<Voice, BaroquenNote> _notes = notes.ToFrozenDictionary(note => note.Voice);
}
