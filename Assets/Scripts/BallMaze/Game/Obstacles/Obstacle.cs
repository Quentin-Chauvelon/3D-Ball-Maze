using System;
using System.Collections.Generic;
using BallMaze.Newtonsoft.Helpers;
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
        FlagTarget,
        CorneredRail,
        Rail,
        HalfSphere,
        HalfCylinder,
        Tunnel,
        Spikes,
        Wedge,
        FloorHole
    }


    /// <summary>
    /// Base class defining an obstacle
    /// </summary>
    [Serializable]
    [JsonConverter(typeof(ObstacleConverter))]
    public abstract class Obstacle
    {
        [JsonProperty("o")]
        public abstract ObstacleType obstacleType { get; }

        public int id;

        [JsonIgnore]
        public abstract bool canRespawnOn { get; }

        [JsonIgnore]
        public abstract bool canRollOn { get; }

        [JsonIgnore]
        public abstract bool canKill { get; }


        public Obstacle(int id)
        {
            this.id = id;
        }


        public abstract GameObject Render(Dictionary<GameObject, Obstacle> obstacles);



        /// <summary>
        /// Utility method to position an obstacle over another obstacle from its id.
        /// Used to position walls over floors, for example.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="obstacleId"></param>
        /// <param name="offset"></param>
        protected void PositionObstacleOverObstacleFromId(Dictionary<GameObject, Obstacle> obstacles, Transform transform, int obstacleId, Vector3 offset)
        {
            foreach (KeyValuePair<GameObject, Obstacle> obstacle in obstacles)
            {
                if (obstacle.Value.id == obstacleId)
                {
                    transform.position = obstacle.Key.transform.position + offset;
                    return;
                }
            }

            Debug.LogWarning($"Could not find obstacle with id {obstacleId} to position {transform.name} over.");
        }
    }
}