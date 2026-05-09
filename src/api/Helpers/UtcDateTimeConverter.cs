using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Ensures all DateTime values are serialized as UTC (with trailing Z),
/// fixing the MySQL/EF Core issue where DateTime.Kind is Unspecified.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
