using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class Vector3Converter : JsonConverter<Vector3>
{
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }

    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float x = 0, y = 0, z = 0;

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
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                break;
            }
        }

        return new Vector3(x, y, z);
    }
}