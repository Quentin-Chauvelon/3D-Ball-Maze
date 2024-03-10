using System;
using UnityEngine;
using UnityExtensionMethods;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple wall
    /// </summary>
    public class Wall : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Wall;

        public override bool canRespawnOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }


        public Wall(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }
    }
}