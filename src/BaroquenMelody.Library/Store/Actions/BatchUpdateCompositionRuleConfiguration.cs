using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record BatchUpdateCompositionRuleConfiguration(IDictionary<CompositionRule, CompositionRuleConfiguration> Configurations);
