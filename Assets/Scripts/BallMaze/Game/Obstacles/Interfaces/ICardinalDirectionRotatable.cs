using Newtonsoft.Json;
using UnityEngine;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Interface for obstacles that can only be rotated in 4 directions (North, East, South, West)
    /// </summary>
    public interface ICardinalDirectionRotatable
    {
        /// <summary>
        /// The direction the obstacle is facing
        /// </summary>
        [JsonProperty("d")]
        CardinalDirection direction { get; set; }
    }
}