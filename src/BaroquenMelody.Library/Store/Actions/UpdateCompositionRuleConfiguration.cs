using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionRuleConfiguration(
    CompositionRule CompositionRule,
    ConfigurationStatus Status,
    int Strictness
);
