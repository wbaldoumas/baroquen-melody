using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;

internal sealed record OrnamentationProcessorConfiguration(
    OrnamentationType OrnamentationType,
    IInputPolicy<OrnamentationItem>[] InputPolicies,
    IOutputPolicy<OrnamentationItem>[] OutputPolicies,
    int[] Translations,
    Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldInvertTranslations,
    FrozenSet<int> TranslationInversionIndices,
    bool ShouldTranslateOnCurrentNote = true
);
