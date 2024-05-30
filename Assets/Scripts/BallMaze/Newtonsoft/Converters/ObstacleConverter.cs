using System;
using System.Collections;
using System.Collections.Generic;
using BallMaze.Obstacles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace BallMaze.Newtonsoft.Helpers
{
    /// <summary>
    /// Class to convert an obstacle from JSON to the appropriate subclass (eg: floor, wall...).
    /// </summary>
    public class ObstacleConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Not implemented yet");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                JContainer lJContainer = JObject.Load(reader);

                if (lJContainer["o"] != null)
                {
                    ObstacleType obstacleType = (ObstacleType)lJContainer["o"].Value<int>();

                    _ = Convert.ChangeType(existingValue, objectType);

                    switch (obstacleType)
                    {
                        case ObstacleType.Floor:
                            existingValue = new Floor(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }));
                            break;
                        case ObstacleType.Wall:
                            existingValue = new Wall(lJContainer["id"].Value<int>(), lJContainer["oId"].Value<int>(), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.Corner:
                            existingValue = new Corner(lJContainer["id"].Value<int>(), lJContainer["oId"].Value<int>(), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.FlagTarget:
                            existingValue = new FlagTarget(lJContainer["id"].Value<int>(), lJContainer["oId"].Value<int>(), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.CorneredRail:
                            existingValue = new CorneredRail(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.Rail:
                            existingValue = new Rail(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.HalfSphere:
                            existingValue = new HalfSphere(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }));
                            break;
                        case ObstacleType.HalfCylinder:
                            existingValue = new HalfCylinder(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.Tunnel:
                            existingValue = new Tunnel(lJContainer["id"].Value<int>(), (TunnelType)lJContainer["tt"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        case ObstacleType.Spikes:
                            existingValue = new Spikes(lJContainer["id"].Value<int>(), lJContainer["oId"].Value<int>());
                            break;
                        case ObstacleType.Wedge:
                            existingValue = new Wedge(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } }), (CardinalDirection)lJContainer["d"].Value<int>()); ;
                            break;
                        case ObstacleType.FloorHole:
                            existingValue = new FloorHole(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } })); ;
                            break;
                        case ObstacleType.KillFloor:
                            existingValue = new KillFloor(lJContainer["id"].Value<int>(), lJContainer["p"].ToObject<Vector3>(new JsonSerializer { Converters = { new Vector3Converter() } })); ;
                            break;
                        case ObstacleType.KillWall:
                            existingValue = new KillWall(lJContainer["id"].Value<int>(), lJContainer["oId"].Value<int>(), (CardinalDirection)lJContainer["d"].Value<int>());
                            break;
                        default:
                            throw new JsonSerializationException($"Unknown obstacle type {obstacleType}");
                    }

                    if (existingValue != null)
                    {
                        serializer.Populate(lJContainer.CreateReader(), existingValue);
                    }
                }
            }

            return existingValue;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Obstacle).IsAssignableFrom(objectType) && objectType.IsSubclassOf(typeof(Obstacle));
        }
    }
}