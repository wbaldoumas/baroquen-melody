namespace BaroquenMelody.Library.Configurations;

public sealed record SavedCompositionConfiguration(
    CompositionConfiguration Configuration,
    FileInfo ConfigurationFile
);
