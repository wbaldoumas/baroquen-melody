using BaroquenMelody.Library.Compositions.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Domain;

internal sealed class Chord(IEnumerable<Note> notes)
{
    public IEnumerable<Note> Notes => _notes.Values;

    public Note this[Voice voice] => _notes[voice];

    private readonly FrozenDictionary<Voice, Note> _notes = notes.ToFrozenDictionary(note => note.Voice);
}
