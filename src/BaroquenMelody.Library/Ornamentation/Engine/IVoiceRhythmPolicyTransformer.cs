using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine;

/// <summary>
///     Substitutes role-aware probability gates into the ornamentation and sustain engines when per-voice
///     rhythm roles are enabled, and leaves the engines bit-for-bit untouched when they are not.
/// </summary>
internal interface IVoiceRhythmPolicyTransformer
{
    /// <summary>
    ///     Replace each <see cref="Policies.Input.WantsToOrnament"/> gate in the given input policies — wherever
    ///     it sits — with its role-aware sibling carrying the same base weight. All other policies pass through
    ///     untouched, as does the entire array when roles are disabled.
    /// </summary>
    /// <param name="ornamentationConfiguration">The ornamentation configuration the policies gate, carrying its type and base probability.</param>
    /// <param name="inputPolicies">The processor's input policies.</param>
    /// <returns>The input policies to build the processor with.</returns>
    IInputPolicy<OrnamentationItem>[] Transform(OrnamentationConfiguration ornamentationConfiguration, IInputPolicy<OrnamentationItem>[] inputPolicies);

    /// <summary>
    ///     Create the sustain engine's probability gate: role-aware (held pairs tie deterministically) when
    ///     roles are enabled, the standard gate otherwise.
    /// </summary>
    /// <returns>The input policy gating the sustain engine.</returns>
    IInputPolicy<OrnamentationItem> CreateSustainGate();
}
