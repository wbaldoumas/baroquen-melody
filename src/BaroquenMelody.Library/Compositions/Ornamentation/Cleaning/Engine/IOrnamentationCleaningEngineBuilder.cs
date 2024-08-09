using Atrea.PolicyEngine;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;

/// <summary>
///     Builds a policy engine for cleaning ornamentations.
/// </summary>
internal interface IOrnamentationCleaningEngineBuilder
{
    /// <summary>
    ///     Build a policy engine for cleaning ornamentations.
    /// </summary>
    /// <returns>The policy engine for cleaning ornamentations.</returns>
    IPolicyEngine<OrnamentationCleaningItem> Build();
}
