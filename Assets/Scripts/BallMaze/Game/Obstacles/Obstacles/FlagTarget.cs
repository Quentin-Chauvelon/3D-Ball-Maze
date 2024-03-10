using System;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a flag target
    /// </summary>
    public class FlagTarget : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.FlagTarget;

        public override bool canRespawnOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }

        public FlagTarget(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }
    }
}