using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;

[ExcludeFromCodeCoverage(Justification = "Simple log policy.")]
internal sealed class LogOrnamentation(OrnamentationType ornamentationType, ILogger logger) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item) => logger.AppliedOrnamentation(ornamentationType, item.Instrument);
}
