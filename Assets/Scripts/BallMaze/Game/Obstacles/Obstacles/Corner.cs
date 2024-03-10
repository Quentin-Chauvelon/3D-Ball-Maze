using System;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple wall corner, which allows to connect perpendicular walls
    /// </summary>
    public class Corner : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Corner;

        public override bool canRespawnOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }

        public Corner(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }
    }
}