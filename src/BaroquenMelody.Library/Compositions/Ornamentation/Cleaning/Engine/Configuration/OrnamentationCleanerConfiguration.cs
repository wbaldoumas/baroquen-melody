using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Configuration;

/// <summary>
///     A configuration for ornamentation cleaning.
/// </summary>
/// <param name="NotePairSelector">A note selector, which selects the primary and secondary notes to consider for ornamentation cleaning.</param>
/// <param name="NoteIndexPairs">The pairs of indices of the notes to consider for ornamentation cleaning.</param>
/// <param name="NoteTargetSelector">A cleaning selector, which selects the note to clean ornamentations from.</param>
internal sealed record OrnamentationCleanerConfiguration(
    INotePairSelector NotePairSelector,
    IEnumerable<NoteIndexPair> NoteIndexPairs,
    INoteTargetSelector NoteTargetSelector
);
