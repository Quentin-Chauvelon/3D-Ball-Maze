using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BallMaze.Obstacles
{
    /// <summary>
    /// Class defining a simple wall corner, which allows to connect perpendicular walls
    /// </summary>
    public class Corner : Obstacle, IRelativelyPositionnable, ICardinalDirectionRotatable
    {
        public override ObstacleType obstacleType => ObstacleType.Corner;

        public override bool canRespawnOn => false;

        public override bool canRollOn => true;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }


        public Corner(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles)
        {
            GameObject corner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            corner.name = "Corner";
            corner.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);

            Vector3 offset = Vector3.zero;
            // Add an offset to move the wall to the correct position based on the direction it's facing
            switch (direction)
            {
                case CardinalDirection.North:
                    offset = new Vector3(-0.45f, 0, -0.45f);
                    break;
                case CardinalDirection.East:
                    offset = new Vector3(-0.45f, 0, 0.45f);
                    break;
                case CardinalDirection.South:
                    offset = new Vector3(0.45f, 0, 0.45f);
                    break;
                case CardinalDirection.West:
                    offset = new Vector3(0.45f, 0, -0.45f);
                    break;
                default:
                    Debug.LogWarning($"Invalid wall direction {direction} for wall {id}.");
                    break;
            }

            PositionObstacleOverObstacleFromId(obstacles, corner.transform, obstacleId, offset + new Vector3(0, 0.2f, 0));

            corner.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return corner;
        }
    }
}