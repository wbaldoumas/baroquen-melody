using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;

namespace BaroquenMelody.App.Components.Tests.TestData;

/// <summary>
///     Drives the instrument ranges into and out of voice-spacing satisfiability, mirroring a user
///     dragging the top voice's tonal range far above the other voices and back.
/// </summary>
internal static class VoiceSpacingScenarios
{
    /// <summary>
    ///     Moves the top voice's range to the top of the scale, far above the second voice, so the
    ///     voice spacing rule cannot be satisfied. Returns the original configuration for restoring.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    /// <returns>The top voice's configuration before the move.</returns>
    public static InstrumentConfiguration MoveTopVoiceOutOfSatisfiableRange(AppComponentsTestContext testContext)
    {
        var originalConfiguration = testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;
        var availableNotes = testContext.StateOf<CompositionConfigurationState>().AvailableNotes;

        testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.One, availableNotes[^8], availableNotes[^1]));

        return originalConfiguration;
    }

    /// <summary>
    ///     Restores the top voice's range, making the voice spacing rule satisfiable again.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    /// <param name="originalConfiguration">The configuration captured before the move.</param>
    public static void RestoreTopVoice(AppComponentsTestContext testContext, InstrumentConfiguration originalConfiguration) =>
        testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.One, originalConfiguration.MinNote, originalConfiguration.MaxNote));
}
