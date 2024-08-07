﻿using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

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

    /// <summary>
    ///     Decorate the given voice in the composition.
    /// </summary>
    /// <param name="composition">The composition to decorate.</param>
    /// <param name="voice">The voice to decorate.</param>
    void Decorate(Composition composition, Voice voice);

    /// <summary>
    ///    Apply sustain to the composition by identifying repeated notes and extending their musical time span.
    /// </summary>
    /// <param name="composition">The composition to apply sustain to.</param>
    void ApplySustain(Composition composition);
}
