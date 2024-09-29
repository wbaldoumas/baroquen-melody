using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Infrastructure.Logging;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;

[ExcludeFromCodeCoverage(Justification = "Simple log policy.")]
internal sealed class LogOrnamentation(OrnamentationType ornamentationType, ILogger logger) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item) => logger.LogInfoMessage($"Ornamentation {ornamentationType} applied to instrument {item.Instrument}.");
}
