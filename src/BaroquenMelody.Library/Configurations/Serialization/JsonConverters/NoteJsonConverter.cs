using Melanchall.DryWetMidi.MusicTheory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Configurations.Serialization.JsonConverters;

public sealed class NoteJsonConverter : JsonConverter<Note>
{
    public override Note? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => Note.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, Note value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
