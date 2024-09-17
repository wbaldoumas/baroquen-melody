namespace BaroquenMelody.Library.Compositions.Configurations;

public sealed record SavedCompositionConfiguration(
    CompositionConfiguration Configuration,
    FileInfo ConfigurationFile
);
