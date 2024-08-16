using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class CompositionConfigurationService(
    IOrnamentationConfigurationService compositionOrnamentationConfigurationService,
    ICompositionRuleConfigurationService compositionRuleConfigurationService,
    IVoiceConfigurationService compositionVoiceConfigurationService,
    IDispatcher dispatcher
) : ICompositionConfigurationService
{
    private const Meter _defaultMeter = Meter.FourFour;

    private const NoteName _defaultRootNote = NoteName.C;

    private const Mode _defaultMode = Mode.Ionian;

    private static readonly FrozenSet<NoteName> _configurableRootNotes = EnumUtils<NoteName>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Mode> _configurableScaleModes = EnumUtils<Mode>.AsEnumerable().ToFrozenSet();

    private static readonly FrozenSet<Meter> _configurableMeters = EnumUtils<Meter>.AsEnumerable().ToFrozenSet();

    public IEnumerable<NoteName> ConfigurableRootNotes => _configurableRootNotes;

    public IEnumerable<Mode> ConfigurableScaleModes => _configurableScaleModes;

    public IEnumerable<Meter> ConfigurableMeters => _configurableMeters;

    public void ConfigureDefaults()
    {
        compositionOrnamentationConfigurationService.ConfigureDefaults();
        compositionRuleConfigurationService.ConfigureDefaults();
        compositionVoiceConfigurationService.ConfigureDefaults();

        dispatcher.Dispatch(new UpdateCompositionConfiguration(_defaultRootNote, _defaultMode, _defaultMeter));
    }
}
