using Newtonsoft.Json;

namespace LibraryOfTroyApi.DTOs;

// Custom converter for Guid to TypeScript Guid
public class TypeScriptGuidConverter : JsonConverter {
    public override bool CanConvert ( Type objectType ) {
        return objectType == typeof ( Guid );
    }

    public override object ReadJson ( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer ) {
        // Handle string GUID in JSON format
        if ( reader.TokenType == JsonToken.String ) {
            string guidString = (string)reader.Value;
            if ( Guid.TryParse ( guidString, out Guid result ) ) {
                return result;
            }
        }

        return Guid.Empty;
    }

    public override void WriteJson ( JsonWriter writer, object value, JsonSerializer serializer ) {
        string? stringValue = value?.ToString();

        if ( stringValue is null ) {
            writer.WriteNull ( );
        } else {
            writer.WriteValue ( stringValue.ToLowerInvariant ( ) );
        }
    }
}
