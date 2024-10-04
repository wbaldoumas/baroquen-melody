using Melanchall.DryWetMidi.Common;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Configurations.Serialization.JsonConverters;

public sealed class SevenBitNumberJsonConverter : JsonConverter<SevenBitNumber>
{
    private static readonly SevenBitNumber DefaultSevenBitNumber = new(50);

    public override SevenBitNumber Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();

        if (SevenBitNumber.TryParse(value, out var sevenBitNumber))
        {
            return sevenBitNumber;
        }

        return DefaultSevenBitNumber;
    }

    public override void Write(Utf8JsonWriter writer, SevenBitNumber value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
