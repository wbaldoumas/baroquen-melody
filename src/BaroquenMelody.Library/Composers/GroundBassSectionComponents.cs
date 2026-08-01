using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;

namespace BaroquenMelody.Library.Composers;

/// <summary>
///     The key-bound components a tonal section composes with: the full-rule search strategy, the seam
///     strategy used for the one transition that crosses into the section from another key, the selector
///     whose scoring reads the section's harmony, and the decoration and tonicization passes built against
///     the section's scale.
/// </summary>
/// <param name="FullStrategy"> The strategy enforcing the full effective rule set in the section's key. </param>
/// <param name="SeamStrategy">
///     The strategy for transitions arriving from another key: the full effective rule set minus the
///     standard-progression rule, whose grammar is intra-key by definition and has no jurisdiction across a
///     section boundary (every voice-leading rule stays enforced). Aliases <paramref name="FullStrategy"/>
///     when no modulation can occur, since no cross-key transition can then exist.
/// </param>
/// <param name="ChordSelector"> The selector ranking rule-valid candidates with the section-keyed scoring. </param>
/// <param name="Decorator"> The ornamentation decorator resolving figures against the section's scale. </param>
/// <param name="TonicizationApplicator"> The tonicization pass carrying the section's mode-derived licenses. </param>
internal sealed record GroundBassSectionComponents(
    ICompositionStrategy FullStrategy,
    ICompositionStrategy SeamStrategy,
    IChordSelector ChordSelector,
    ICompositionDecorator Decorator,
    ITonicizationApplicator TonicizationApplicator);
