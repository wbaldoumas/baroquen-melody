using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Ornamentation;

/// <summary>
///     Represents something that can decorate a composition.
/// </summary>
internal interface ICompositionDecorator
{
    /// <summary>
    ///     Decorate the composition.
    /// </summary>
    /// <param name="composition">The composition to decorate.</param>
    void Decorate(Composition composition);
}
