namespace BaroquenMelody.Library.Enums;

/// <summary>
///     Represents the steps in the composition process.
/// </summary>
public enum CompositionStep : byte
{
    /// <summary>
    ///     The composer is waiting to start composing.
    /// </summary>
    Waiting,

    /// <summary>
    ///     The composer is composing the theme.
    /// </summary>
    Theme,

    /// <summary>
    ///     The composer is composing the body of the composition.
    /// </summary>
    Body,

    /// <summary>
    ///     The composer is adding ornamentation to the composition.
    /// </summary>
    Ornamentation,

    /// <summary>
    ///     The composer is phrasing the composition.
    /// </summary>
    Phrasing,

    /// <summary>
    ///     The composer is adding an ending to the composition.
    /// </summary>
    Ending,

    /// <summary>
    ///     The composer has completed composing the composition.
    /// </summary>
    Complete,

    /// <summary>
    ///     The composer has failed to compose the composition.
    /// </summary>
    Failed
}
