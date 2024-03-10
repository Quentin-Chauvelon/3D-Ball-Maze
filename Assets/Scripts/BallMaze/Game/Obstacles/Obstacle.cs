using System;
using Newtonsoft.Json;
using UnityEngine;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// The possible types of obstacles.
    /// </summary>
    public enum ObstacleType
    {
        Floor,
        Wall,
        Corner,
        FlagTarget
    }


    /// <summary>
    /// Base class defining an obstacle
    /// </summary>
    [Serializable]
    public abstract class Obstacle
    {
        [JsonProperty("o")]
        public abstract ObstacleType obstacleType { get; }

        public int id;

        [JsonIgnore]
        public abstract bool canRespawnOn { get; }

        [JsonIgnore]
        public abstract bool canKill { get; }


        public Obstacle(int id)
        {
            this.id = id;
        }
    }
}