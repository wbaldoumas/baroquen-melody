using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     Schedules the ground bass form's division roles: which upper voice carries each accompanied
///     statement's figuration, how intensely each statement escalates, how far each statement's dynamics
///     terrace below the close, and whether the ground line takes the sustained tread.
/// </summary>
internal interface IGroundBassDivisionScheduler
{
    /// <summary>
    ///     Determine whether the ground line's notes take the held role, tying its sustain-eligible
    ///     repeated pairs deterministically — the announcement included.
    /// </summary>
    /// <returns>Whether the ground line holds.</returns>
    bool ShouldHoldGroundLine();

    /// <summary>
    ///     Determine the escalation intensity for an accompanied statement: the percentage scale applied to
    ///     the statement's upper-voice ornamentation gate weights.
    /// </summary>
    /// <param name="plan">The ground bass plan.</param>
    /// <param name="statementIndex">The zero-based statement index.</param>
    /// <param name="intensity">The statement's intensity, when the statement escalates.</param>
    /// <returns>Whether the statement escalates.</returns>
    bool TryGetIntensity(GroundBassPlan plan, int statementIndex, out int intensity);

    /// <summary>
    ///     Determine which upper voice carries an accompanied statement's figuration concentration.
    /// </summary>
    /// <param name="plan">The ground bass plan.</param>
    /// <param name="statementIndex">The zero-based statement index.</param>
    /// <param name="floridInstrument">The statement's florid voice, when the statement escalates.</param>
    /// <returns>Whether the statement carries a florid voice.</returns>
    bool TryGetFloridInstrument(GroundBassPlan plan, int statementIndex, out Instrument floridInstrument);

    /// <summary>
    ///     Determine how far an accompanied statement's velocities terrace below today's level: a
    ///     non-positive offset reaching zero at the final statement, so the arc grows into the close.
    /// </summary>
    /// <param name="plan">The ground bass plan.</param>
    /// <param name="statementIndex">The zero-based statement index.</param>
    /// <param name="terraceOffset">The statement's velocity offset, when the statement escalates.</param>
    /// <returns>Whether the statement terraces.</returns>
    bool TryGetTerraceOffset(GroundBassPlan plan, int statementIndex, out int terraceOffset);
}
