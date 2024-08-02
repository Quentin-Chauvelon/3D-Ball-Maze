using System.Collections.Generic;
using Newtonsoft.Json;


namespace BallMaze.Newtonsoft.Helpers
{
    /// <summary>
    /// Class to configure the JSON settings for the application.
    /// see: https://forum.unity.com/threads/jsonserializationexception-self-referencing-loop-detected.1264253/#post-9490090
    /// </summary>
    public static class CustomJsonSettings
    {
        public static void ConfigureJsonInternal()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>
                {
                    new Vector3Converter(),
                    new QuaternionConverter()
                }
            };
        }
    }
}