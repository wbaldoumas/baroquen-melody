using Atrea.PolicyEngine.Policies.Input;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;

/// <summary>
///     A configuration for an ornamentation processor.
/// </summary>
/// <param name="OrnamentationType">The type of ornamentation to apply.</param>
/// <param name="InputPolicies">The input policies gating the application of the ornamentation.</param>
/// <param name="OutputPolicies">The output policies applied after the ornamentation is applied.</param>
/// <param name="Translations">The note translations to apply in order to generate the ornamentation.</param>
/// <param name="ShouldInvertTranslations">A predicate determining if the translations should be inverted.</param>
/// <param name="TranslationInversionIndices">When translations are inverted, the indices of translations to invert.</param>
/// <param name="ShouldTranslateOnCurrentNote">Whether to translate on the current note or the next note.</param>
internal sealed record OrnamentationProcessorConfiguration(
    OrnamentationType OrnamentationType,
    IInputPolicy<OrnamentationItem>[] InputPolicies,
    IOutputPolicy<OrnamentationItem>[] OutputPolicies,
    int[] Translations,
    Predicate<(BaroquenNote? CurrentNote, BaroquenNote? NextNote)> ShouldInvertTranslations,
    FrozenSet<int> TranslationInversionIndices,
    bool ShouldTranslateOnCurrentNote = true
);
