using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Builders;
using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.MusicTheory;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine;

[ExcludeFromCodeCoverage(Justification = "Trivial builder methods.")]
internal sealed class OrnamentationEngineBuilder(CompositionConfiguration compositionConfiguration, IMusicalTimeSpanCalculator musicalTimeSpanCalculator)
{
    private const int DecorateDominantSeventhAboveSupertonicInterval = 3;
    private const int DecorateDominantSeventhBelowSupertonicInterval = -4;
    private const int DecorateDominantSeventhAboveLeadingToneInterval = 5;
    private const int DecorateDominantSeventhBelowLeadingToneInterval = -2;

    private readonly IChordNumberIdentifier _chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

    public IPolicyEngine<OrnamentationItem> BuildOrnamentationEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithoutInputPolicies()
        .WithProcessors(
            BuildPassingToneEngine(),
            BuildDoublePassingToneEngine(),
            BuildDelayedDoublePassingToneEngine(),
            BuildDoubleTurnEngine(),
            BuildDelayedPassingToneEngine(),
            BuildSixteenthNoteRunEngine(),
            BuildDoubleThirtySecondNoteRunProcessor(),
            BuildTurnEngine(),
            BuildAlternateTurnEngine(),
            BuildDelayedThirtySecondNoteRunEngine(),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhBelowSupertonicInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.Supertonic, DecorateDominantSeventhAboveSupertonicInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhAboveLeadingToneInterval),
            BuildDecorateDominantSeventhIntervalEngine(compositionConfiguration.Scale.LeadingTone, DecorateDominantSeventhBelowLeadingToneInterval),
            BuildPedalProcessor(new IsRootOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.RootPedalInterval),
            BuildPedalProcessor(new IsThirdOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.ThirdPedalInterval),
            BuildPedalProcessor(new IsFifthOfChord(_chordNumberIdentifier, compositionConfiguration), PedalProcessor.FifthPedalInterval)
        )
        .WithOutputPolicies(new CleanConflictingOrnamentations(new OrnamentationCleanerFactory()))
        .Build();

    public IPolicyEngine<OrnamentationItem> BuildSustainedNoteEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new IsRepeatedNote(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, SustainedNoteProcessor.Interval)
        )
        .WithProcessors(new SustainedNoteProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.PassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedPassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, PassingToneProcessor.Interval)
        )
        .WithProcessors(new PassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedPassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, TurnProcessor.Interval)
        )
        .WithProcessors(new TurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildAlternateTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, AlternateTurnProcessor.Interval)
        )
        .WithProcessors(new AlternateTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildSixteenthNoteRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, SixteenthNoteRunProcessor.Interval)
        )
        .WithProcessors(new SixteenthNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedThirtySecondNoteRunEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(10),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DelayedThirtySecondNoteRunProcessor.Interval)
        )
        .WithProcessors(new DelayedThirtySecondNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleTurnEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(25),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoubleTurnProcessor.Interval)
        )
        .WithProcessors(new DoubleTurnProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DoublePassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDelayedDoublePassingToneEngine() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, DoublePassingToneProcessor.Interval)
        )
        .WithProcessors(new DoublePassingToneProcessor(musicalTimeSpanCalculator, compositionConfiguration, OrnamentationType.DelayedDoublePassingTone))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDecorateDominantSeventhIntervalEngine(NoteName targetNote, int intervalChange) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new HasNextBeat(),
            new WantsToOrnament(25),
            new NoteHasNoOrnamentation(),
            new NoteIsTargetNote(targetNote),
            new BeatContainsTargetNotes([compositionConfiguration.Scale.Dominant, compositionConfiguration.Scale.LeadingTone, compositionConfiguration.Scale.Supertonic]),
            new NextBeatContainsTargetNotes([compositionConfiguration.Scale.Tonic, compositionConfiguration.Scale.Mediant, compositionConfiguration.Scale.Dominant]),
            new Not<OrnamentationItem>(new BeatContainsTargetOrnamentation(OrnamentationType.DecorateInterval)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, intervalChange)
        )
        .WithProcessors(new DecorateIntervalProcessor(musicalTimeSpanCalculator, compositionConfiguration, intervalChange))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildDoubleThirtySecondNoteRunProcessor() => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(10),
            new NoteHasNoOrnamentation(),
            new IsApplicableInterval(compositionConfiguration, ThirtySecondNoteRunProcessor.Interval)
        )
        .WithProcessors(new ThirtySecondNoteRunProcessor(musicalTimeSpanCalculator, compositionConfiguration))
        .WithoutOutputPolicies()
        .Build();

    private IPolicyEngine<OrnamentationItem> BuildPedalProcessor(IInputPolicy<OrnamentationItem> scaleDegreePolicy, int pedalInterval) => PolicyEngineBuilder<OrnamentationItem>.Configure()
        .WithInputPolicies(
            new WantsToOrnament(),
            new NoteHasNoOrnamentation(),
            scaleDegreePolicy,
            new IsApplicableInterval(compositionConfiguration, PedalProcessor.Interval),
            new Not<OrnamentationItem>(new BeatContainsTargetOrnamentation(OrnamentationType.Pedal)),
            new IsIntervalWithinVoiceRange(compositionConfiguration, pedalInterval)
        )
        .WithProcessors(new PedalProcessor(musicalTimeSpanCalculator, compositionConfiguration, pedalInterval))
        .WithoutOutputPolicies()
        .Build();
}
