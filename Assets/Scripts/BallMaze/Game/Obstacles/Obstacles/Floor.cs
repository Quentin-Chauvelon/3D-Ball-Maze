using System;
using UnityEngine;

namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple floor tile
    /// </summary>
    [Serializable]
    public class Floor : Obstacle, IAbsolutelyPositionnable
    {
        public override ObstacleType obstacleType => ObstacleType.Floor;

        public override bool canRespawnOn => true;

        public override bool canKill => false;

        public Vector3 position { get; set; }

        public Floor(int id, Vector3 position) : base(id)
        {
            this.position = position;
        }
    }
}
