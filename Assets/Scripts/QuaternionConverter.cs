using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class QuaternionConverter : JsonConverter<Quaternion>
{
    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {
        // Serializáljuk a Quaternion x, y, z, w komponenseit
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WritePropertyName("w");
        writer.WriteValue(value.w);
        writer.WriteEndObject();
    }

    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float x = 0, y = 0, z = 0, w = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var propertyName = reader.Value.ToString();
                if (!reader.Read()) continue;

                switch (propertyName)
                {
                    case "x":
                        x = Convert.ToSingle(reader.Value);
                        break;
                    case "y":
                        y = Convert.ToSingle(reader.Value);
                        break;
                    case "z":
                        z = Convert.ToSingle(reader.Value);
                        break;
                    case "w":
                        w = Convert.ToSingle(reader.Value);
                        break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                break;
            }
        }

        return new Quaternion(x, y, z, w);
    }
}
