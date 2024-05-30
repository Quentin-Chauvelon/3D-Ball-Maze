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
    }
}