using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Compositions.Configurations.Serialization.JsonSerializerContexts;

/// <summary>
///     Provides a context for serializing and deserializing <see cref="CompositionConfiguration"/> objects.
/// </summary>
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(CompositionConfiguration))]
public partial class CompositionConfigurationJsonSerializerContext : JsonSerializerContext;
