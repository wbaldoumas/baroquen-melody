using BaroquenMelody.Library.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionProgressState(
    ISet<CompositionStep> CompletedSteps,
    CompositionStep CurrentStep,
    double ThemeProgress = 0.0d,
    double BodyProgress = 0.0d,
    double EndingProgress = 0.0d
)
{
    public double OverallProgress => (ThemeProgress + BodyProgress + EndingProgress) / 3;

    public bool IsComplete => CurrentStep == CompositionStep.Complete;

    public bool IsWaiting => CurrentStep == CompositionStep.Waiting;

    public bool IsLoading => !IsComplete && !IsWaiting && !IsFailed;

    public bool IsFailed => CurrentStep == CompositionStep.Failed;

    public string Message => CurrentStep switch
    {
        CompositionStep.Waiting => "Waiting to compose...",
        CompositionStep.Theme => "Composing main theme...",
        CompositionStep.Body => "Continuing composition...",
        CompositionStep.Ornamentation => "Composing ending...", // Step completes too quickly to show a different message to the user.
        CompositionStep.Phrasing => "Composing ending...", // Step completes too quickly to show a different message to the user.
        CompositionStep.Ending => "Composing ending...",
        CompositionStep.Complete => "Composition complete!",
        CompositionStep.Failed => "Failed to compose composition.",
        _ => throw new ArgumentOutOfRangeException(nameof(CurrentStep), $"Unknown composition step: {CurrentStep}.")
    };

    public CompositionProgressState()
        : this(new HashSet<CompositionStep>(), CompositionStep.Waiting)
    {
    }
}
