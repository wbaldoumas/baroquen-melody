using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Forms.Enums;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     Determines which patterns from the built-in ground bass bank can anchor their tonic inside the lowest
///     voice's range for a given key. The planner draws from exactly this set when composing in the ground
///     bass form, so the UI can warn ahead of composition when a range or key change shrinks the repertoire
///     or empties it entirely (in which case the composition falls back to the fugue form).
/// </summary>
public interface IGroundBassFeasibilityAnalyzer
{
    /// <summary>
    ///     Gets the built-in ground bass bank's pattern identifiers, in bank order.
    /// </summary>
    IReadOnlyList<GroundBass> GroundBassBank { get; }

    /// <summary>
    ///     Determines which ground bass patterns can anchor inside the lowest voice's range for the given
    ///     composition configuration.
    /// </summary>
    /// <param name="compositionConfiguration">The composition configuration to analyze.</param>
    /// <returns>The feasible patterns, in bank order.</returns>
    IReadOnlyList<GroundBass> GetFeasibleGroundBasses(CompositionConfiguration compositionConfiguration);

    /// <summary>
    ///     Determines which ground bass patterns can anchor inside the lowest voice's range for the given
    ///     instrument configurations and scale. Callers pass the voices that will sound (disabled voices
    ///     excluded); an empty set yields an empty result.
    /// </summary>
    /// <param name="instrumentConfigurations">The instrument configurations whose lowest voice hosts the ground.</param>
    /// <param name="scale">The scale supplying the tonic anchors and rendered notes.</param>
    /// <returns>The feasible patterns, in bank order.</returns>
    IReadOnlyList<GroundBass> GetFeasibleGroundBasses(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale);

    /// <summary>
    ///     Determines whether a ground bass composition would actually state a ground, mirroring the
    ///     planner's own decision: a specific configured pattern must itself fit the lowest voice's range,
    ///     while the composer's free draw (<see langword="null"/>) needs any feasible pattern at all.
    /// </summary>
    /// <param name="instrumentConfigurations">The instrument configurations whose lowest voice hosts the ground.</param>
    /// <param name="scale">The scale supplying the tonic anchors and rendered notes.</param>
    /// <param name="pattern">The configured pattern, or <see langword="null"/> for the composer's free draw.</param>
    /// <returns>Whether a ground bass composition would state a ground rather than fall back to the fugue.</returns>
    bool HasFeasibleGround(IEnumerable<InstrumentConfiguration> instrumentConfigurations, BaroquenScale scale, GroundBass? pattern);
}
