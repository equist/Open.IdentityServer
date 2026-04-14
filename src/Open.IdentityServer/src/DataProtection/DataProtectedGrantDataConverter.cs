#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Open.IdentityServer.Storage.Stores.DataProtection;

namespace Open.IdentityServer.Stores.Serialization;

public class DataProtectedGrantDataConverter: JsonConverter<DataProtectedGrantData>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override DataProtectedGrantData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        DataProtectedGrantData? result = null;
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var property = reader.GetString();
                reader.Read();
                
                if (property == null) continue;

                if (property.Equals(nameof(DataProtectedGrantData.PersistentGrantDataContainerVersion), StringComparison.OrdinalIgnoreCase))
                {
                    result ??= new DataProtectedGrantData();
                    result.PersistentGrantDataContainerVersion = reader.GetInt32();
                }
                if (property.Equals(nameof(DataProtectedGrantData.DataProtected), StringComparison.OrdinalIgnoreCase))
                {
                    result ??= new DataProtectedGrantData();
                    result.DataProtected = reader.GetBoolean();
                }
                if (property.Equals(nameof(DataProtectedGrantData.Payload), StringComparison.OrdinalIgnoreCase))
                {
                    result ??= new DataProtectedGrantData();
                    result.Payload = reader.GetString();
                }
            }
        }

        return result;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DataProtectedGrantData value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value);
    }
}