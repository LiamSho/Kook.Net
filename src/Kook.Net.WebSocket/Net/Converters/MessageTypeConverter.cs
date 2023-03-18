using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kook.Net.Converters;

internal class MessageTypeConverter : JsonConverter<MessageType>
{
    public override MessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => (MessageType)reader.GetInt32();

    public override void Write(Utf8JsonWriter writer, MessageType value, JsonSerializerOptions options) => writer.WriteNumberValue((int)value);
}
