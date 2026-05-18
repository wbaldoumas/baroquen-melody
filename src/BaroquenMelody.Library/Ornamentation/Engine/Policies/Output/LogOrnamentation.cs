using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Logging;
using BaroquenMelody.Library.Ornamentation.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;

[ExcludeFromCodeCoverage(Justification = "Simple log policy.")]
internal sealed class LogOrnamentation(OrnamentationType ornamentationType, ILogger logger) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item) => logger.LogOrnamentationApplied(ornamentationType, item.Instrument);
}
