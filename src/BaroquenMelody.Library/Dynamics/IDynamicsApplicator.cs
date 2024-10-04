using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Dynamics;

/// <summary>
///     Represents something that can apply dynamics to a composition.
/// </summary>
internal interface IDynamicsApplicator
{
    /// <summary>
    ///     Applies dynamics to the given composition.
    /// </summary>
    /// <param name="composition">The composition to apply dynamics to.</param>
    void Apply(Composition composition);
}
