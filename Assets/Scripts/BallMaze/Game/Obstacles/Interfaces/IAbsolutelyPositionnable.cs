using System;
using Newtonsoft.Json;
using UnityEngine;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Interface for obstacles that can be positioned anywhere, using an absolute position (Vector3)
    /// </summary>
    public interface IAbsolutelyPositionnable
    {
        /// <summary>
        /// The position of the obstacle
        /// </summary>
        [JsonProperty("p")]
        Vector3 position { get; set; }

        /// <summary>
        /// An offset that can be used when wanting to position the obstacle away from the origin.
        /// This is useful since updating the obstacle's position will break the obstaclesTypesMap used to build and render the maze
        /// </summary>
        [JsonIgnore]
        Vector3 offset { get; set; }
    }
}