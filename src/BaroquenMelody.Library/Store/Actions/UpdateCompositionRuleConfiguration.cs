using BaroquenMelody.Library.Compositions.Configurations.Enums;
using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionRuleConfiguration(
    CompositionRule CompositionRule,
    ConfigurationStatus Status,
    int Strictness
);
