using Melanchall.DryWetMidi.Interaction;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Compositions.Configurations.Serialization.JsonConverters;

public sealed class MusicalTimespanJsonConverter : JsonConverter<MusicalTimeSpan>
{
    public override MusicalTimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => MusicalTimeSpan.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, MusicalTimeSpan value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
