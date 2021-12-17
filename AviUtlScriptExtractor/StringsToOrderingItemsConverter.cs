using System.Text.Json;
using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    internal class StringsToOrderingItemsConverter : JsonConverter<List<OrderingItem>>
    {
        public override List<OrderingItem>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<List<string>>(ref reader, options)
                ?.Select(x => new OrderingItem(x))
                ?.ToList();
        }

        public override void Write(Utf8JsonWriter writer, List<OrderingItem> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Select(x => x.ToString()), options);
        }
    }
}
