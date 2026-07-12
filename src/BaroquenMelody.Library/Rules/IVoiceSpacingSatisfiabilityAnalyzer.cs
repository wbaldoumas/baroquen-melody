using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Rules;

/// <summary>
///     Determines whether the voice spacing rule (<see cref="Enums.CompositionRule.EnforceVoiceSpacing"/>) can ever be
///     satisfied by the configured instrument ranges. Some legal range configurations place adjacent upper voices so far
///     apart that no chord voicing can keep them within an octave of each other; enforcing the rule against such a
///     configuration would leave the composer without a single valid chord.
/// </summary>
public interface IVoiceSpacingSatisfiabilityAnalyzer
{
    /// <summary>
    ///     Determines whether at least one chord voicing exists that satisfies the voice spacing rule for the
    ///     given composition configuration's instrument ranges and scale.
    /// </summary>
    /// <param name="compositionConfiguration">The composition configuration to analyze.</param>
    /// <returns>Whether the voice spacing rule is satisfiable.</returns>
    bool IsSatisfiable(CompositionConfiguration compositionConfiguration);

    /// <summary>
    ///     Determines whether at least one chord voicing exists that satisfies the voice spacing rule for the
    ///     given instrument ranges and scale.
    /// </summary>
    /// <param name="instrumentConfigurations">The instrument configurations whose ranges are analyzed.</param>
    /// <param name="scale">The scale supplying the playable notes.</param>
    /// <returns>Whether the voice spacing rule is satisfiable.</returns>
    bool IsSatisfiable(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale);

    /// <summary>
    ///     Retrieves, per instrument, the note numbers for which at least one full chord voicing satisfying the
    ///     voice spacing rule exists. Unconstrained voices (the bass, or every voice when fewer than three are
    ///     configured) retain their full playable range.
    /// </summary>
    /// <param name="compositionConfiguration">The composition configuration to analyze.</param>
    /// <returns>The spacing-feasible note numbers, keyed by instrument.</returns>
    FrozenDictionary<Instrument, FrozenSet<int>> GetFeasibleNoteNumbersByInstrument(CompositionConfiguration compositionConfiguration);
}
