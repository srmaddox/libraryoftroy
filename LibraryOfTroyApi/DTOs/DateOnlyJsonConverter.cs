
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace LibraryOfTroyApi.DTOs;
public class DateOnlyJsonConverter : JsonConverter<DateOnly?> {
    private const string DateFormat = "yyyy-MM-dd"; // Ensures consistent date format

    public override void WriteJson ( JsonWriter writer, DateOnly? value, JsonSerializer serializer ) {
        if ( value.HasValue )
            writer.WriteValue ( value.Value.ToString ( DateFormat, CultureInfo.InvariantCulture ) );
        else
            writer.WriteNull ( );
    }

    public override DateOnly? ReadJson ( JsonReader reader, Type objectType, DateOnly? existingValue, bool hasExistingValue, JsonSerializer serializer ) {
        if ( reader.TokenType == JsonToken.Null )
            return null;

        if ( reader.TokenType == JsonToken.String && DateOnly.TryParseExact ( (string) reader.Value!, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date ) )
            return date;

        throw new JsonSerializationException ( $"Invalid date format: {reader.Value}" );
    }
}

