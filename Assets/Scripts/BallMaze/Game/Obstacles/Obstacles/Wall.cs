using System;
using System.Collections.Generic;
using UnityEditor;
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

        public override bool canRollOn => false;

        public override bool canKill => false;

        public int obstacleId { get; set; }

        public CardinalDirection direction { get; set; }


        public Wall(int id, int obstacleId, CardinalDirection direction) : base(id)
        {
            this.obstacleId = obstacleId;
            this.direction = direction;
        }


        public override GameObject Render(Dictionary<GameObject, Obstacle> obstacles, int[,] obstaclesTypesMap)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";

            // Adapt the wall's scale to the direction it's facing (equivalent to rotating the wall)
            if (direction == CardinalDirection.North || direction == CardinalDirection.South)
            {
                wall.transform.localScale = new Vector3(1, 0.5f, 0.1f);
            }
            else
            {
                wall.transform.localScale = new Vector3(0.1f, 0.5f, 1);
            }

            Vector3 offset = Vector3.zero;
            // Add an offset to move the wall to the correct position based on the direction it's facing
            switch (direction)
            {
                case CardinalDirection.North:
                    offset = new Vector3(0, 0, -0.45f);
                    break;
                case CardinalDirection.East:
                    offset = new Vector3(-0.45f, 0, 0);
                    break;
                case CardinalDirection.South:
                    offset = new Vector3(0, 0, 0.45f);
                    break;
                case CardinalDirection.West:
                    offset = new Vector3(0.45f, 0, 0);
                    break;
                default:
                    Debug.LogWarning($"Invalid wall direction {direction} for wall {id}.");
                    break;
            }

            PositionObstacleOverObstacleFromId(obstacles, wall.transform, obstacleId, offset + new Vector3(0, 0.2f, 0));

            wall.GetComponent<MeshRenderer>().material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Art/Materials/Obstacles/BaseObstacle.mat", typeof(Material));

            return wall;
        }
    }
}