namespace BaroquenMelody.Library.Composition.Choices.Extensions;

/// <summary>
///    Extensions for <see cref="NoteChoice"/>.
/// </summary>
internal static class NoteChoiceExtensions
{
    /// <summary>
    ///     Convert a tuple of <see cref="NoteChoice"/>s to a <see cref="ChordChoice"/>.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteChoice"/>s to convert. </param>
    /// <returns> The <see cref="ChordChoice"/> representing the given tuple of <see cref="NoteChoice"/>s. </returns>
    public static ChordChoice ToChordChoice(this (NoteChoice, NoteChoice) source) =>
        new(new HashSet<NoteChoice> { source.Item1, source.Item2 });

    /// <summary>
    ///     Convert a tuple of <see cref="NoteChoice"/>s to a <see cref="ChordChoice"/>.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteChoice"/>s to convert. </param>
    /// <returns> The <see cref="ChordChoice"/> representing the given tuple of <see cref="NoteChoice"/>s. </returns>
    public static ChordChoice ToChordChoice(this (NoteChoice, NoteChoice, NoteChoice) source) =>
        new(new HashSet<NoteChoice> { source.Item1, source.Item2, source.Item3 });

    /// <summary>
    ///     Convert a tuple of <see cref="NoteChoice"/>s to a <see cref="ChordChoice"/>.
    /// </summary>
    /// <param name="source"> The tuple of <see cref="NoteChoice"/>s to convert. </param>
    /// <returns> The <see cref="ChordChoice"/> representing the given tuple of <see cref="NoteChoice"/>s. </returns>
    public static ChordChoice ToChordChoice(this (NoteChoice, NoteChoice, NoteChoice, NoteChoice) source) =>
        new(new HashSet<NoteChoice> { source.Item1, source.Item2, source.Item3, source.Item4 });
}
