using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.App.Components.Tests.TestData;

/// <summary>
///     Drives the ground bass pattern bank between full, reduced, and empty, mirroring a user editing the
///     ground-hosting voice's range or the composition's key and form. The default lowest enabled voice is
///     Three; a B2-B3 range holds a single C tonic with one scale step below it (no pattern fits), while a
///     G3-B4 range holds C4 with three steps below (only the tetrachord fits).
/// </summary>
internal static class GroundBassScenarios
{
    /// <summary>
    ///     Selects the ground bass form, carrying the rest of the composition configuration forward.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    public static void SelectGroundBassForm(AppComponentsTestContext testContext)
    {
        var state = testContext.StateOf<CompositionConfigurationState>();

        testContext.Dispatcher.Dispatch(new UpdateCompositionConfiguration(state.TonicNote, state.Mode, state.Meter, state.MinimumMeasures, state.Tempo, CompositionForm.GroundBass));
    }

    /// <summary>
    ///     Changes the tonic, carrying the rest of the composition configuration (including the form) forward.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    /// <param name="tonicNote">The new tonic.</param>
    public static void ChangeTonic(AppComponentsTestContext testContext, NoteName tonicNote)
    {
        var state = testContext.StateOf<CompositionConfigurationState>();

        testContext.Dispatcher.Dispatch(new UpdateCompositionConfiguration(tonicNote, state.Mode, state.Meter, state.MinimumMeasures, state.Tempo, state.Form));
    }

    /// <summary>
    ///     Moves the ground-hosting voice into a range no bank pattern can anchor in for a C tonic.
    ///     Returns the original configuration for restoring.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    /// <returns>The ground-hosting voice's configuration before the move.</returns>
    public static InstrumentConfiguration EmptyTheGroundBank(AppComponentsTestContext testContext)
    {
        var originalConfiguration = testContext.StateOf<InstrumentConfigurationState>()[Instrument.Three]!;

        testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.Three, Notes.B2, Notes.B3));

        return originalConfiguration;
    }

    /// <summary>
    ///     Shrinks the ground-hosting voice to a range where only the descending tetrachord can anchor for
    ///     a C tonic.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    public static void ReduceTheGroundBankToTheTetrachord(AppComponentsTestContext testContext) =>
        testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.Three, Notes.G3, Notes.B4));

    /// <summary>
    ///     Restores the ground-hosting voice's range, making the whole bank feasible again.
    /// </summary>
    /// <param name="testContext">The active test context.</param>
    /// <param name="originalConfiguration">The configuration captured before the move.</param>
    public static void RestoreGroundHostingVoice(AppComponentsTestContext testContext, InstrumentConfiguration originalConfiguration) =>
        testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.Three, originalConfiguration.MinNote, originalConfiguration.MaxNote));
}
