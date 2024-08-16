﻿using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionRuleConfiguration(CompositionRule CompositionRule, bool IsEnabled, int Strictness);