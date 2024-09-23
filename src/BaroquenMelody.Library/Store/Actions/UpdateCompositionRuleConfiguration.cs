using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionRuleConfiguration(
    CompositionRule CompositionRule,
    ConfigurationStatus Status,
    int Strictness
);
