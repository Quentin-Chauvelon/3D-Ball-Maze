using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;



namespace BallMaze.Newtonsoft.Helpers
{
    /// <summary>
    /// Custom JSON converter to serialize and deserialize Vector3 objects.
    /// Needed because Unity's objects are not serializable by default with Newtonsoft.Json.
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            float x = jObject.Value<float>("x");
            float y = jObject.Value<float>("y");
            float z = jObject.Value<float>("z");
            return new Vector3(x, y, z);
        }

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
    }
}